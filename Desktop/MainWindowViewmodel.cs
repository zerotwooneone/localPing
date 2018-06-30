using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using System.Windows.Threading;
using Desktop.Dispatcher;
using Desktop.Ping;
using Desktop.Target;
using Desktop.Vector;
using zh.LocalPingLib.Ping;

namespace Desktop
{
    public class MainWindowViewmodel
    {
        private readonly IPingTimer _pingTimer;
        private readonly IPingService _pingService;
        private readonly IPingCollectionVectorFactory _pingCollectionVectorFactory;
        private readonly IPingVectorFactory _pingVectorFactory;
        private readonly IVectorComparer _vectorComparer;
        private readonly PingStatsUtil _pingStatsUtil;
        private readonly IPingResponseUtil _pingResponseUtil;
        private IVector _previousVector;

        public ObservableCollection<TargetDatamodel> TargetDatamodels { get; }
        private readonly IDictionary<IPAddress, PingState> _targetDatamodels;
        private readonly IDictionary<IPAddress, PingStats> _stats;
        private static readonly TimeSpan TimeToShowOddTargets = TimeSpan.FromSeconds(5);
        public IObservable<int> ResortObservable { get; }

        public MainWindowViewmodel(IPingTimer pingTimer,
            IPingService pingService,
            IPingCollectionVectorFactory pingCollectionVectorFactory,
            IPingVectorFactory pingVectorFactory,
            IVectorComparer vectorComparer,
            IIpAddressService ipAddressService,
            IDispatcherAccessor dispatcherAccessor,
            PingStatsUtil pingStatsUtil,
            IPingResponseUtil pingResponseUtil)
        {
            _pingTimer = pingTimer;
            _pingService = pingService;
            _pingCollectionVectorFactory = pingCollectionVectorFactory;
            _pingVectorFactory = pingVectorFactory;
            _vectorComparer = vectorComparer;
            _pingStatsUtil = pingStatsUtil;
            _pingResponseUtil = pingResponseUtil;
            IObservable<long> pingTimerObservable = _pingTimer.Start(() => false);
            IDisposable pingResponseSubscription = null;
            _targetDatamodels = new ConcurrentDictionary<IPAddress, PingState>();
            _stats = new ConcurrentDictionary<IPAddress, PingStats>();
            var d = dispatcherAccessor.GetDispatcher();
            var resortSubject = new Subject<int>();
            ResortObservable = resortSubject;

            ipAddressService.IpAddressObservable.Subscribe(ipAddresses =>
            {
                pingResponseSubscription?.Dispose();
                IObservable<IEnumerable<Task<IPingResponse>>> pingResponseObservable =
                    pingTimerObservable.Select(l => _pingService.Ping(ipAddresses));
                pingResponseSubscription = pingResponseObservable.Subscribe(async pingResponseTasks =>
                {
                    var responses = await Task.WhenAll(pingResponseTasks);
                    var vectors = responses.Select(pingResponse =>
                    {
                        var stats = GetUpdatedStats(pingResponse);
                        var pingVector = _pingVectorFactory.GetVector(pingResponse, stats);
                        if (_targetDatamodels.TryGetValue(pingResponse.TargetIpAddress, out var pingState))
                        {
                            var targetDatamodelX = pingState.TargetDatamodel;
                            targetDatamodelX.RoundTripTime = pingResponse.RoundTripTime;
                            targetDatamodelX.StatusSuccess = GetStatusSuccess(pingResponse.Status);
                            var change = _vectorComparer.Compare(pingState.Previous, pingVector);
                            targetDatamodelX.Change = change;

                            if (targetDatamodelX.Change > 0.008)
                            {
                                targetDatamodelX.ShowUntil = DateTime.Now.Add(TimeToShowOddTargets);
                            }

                            if (targetDatamodelX.ShowUntil <= DateTime.Now)
                            {
                                targetDatamodelX.ShowUntil = null;
                            }

                            pingState.Previous = pingVector;
                        }
                        else
                        {
                            var targetDatamodel = new TargetDatamodel(address: pingResponse.TargetIpAddress,
                                statusSuccess: GetStatusSuccess(pingResponse.Status),
                                roundTripTime: pingResponse.RoundTripTime);
                            _targetDatamodels.Add(pingResponse.TargetIpAddress, new PingState { TargetDatamodel = targetDatamodel, Previous = pingVector });
                            d.Invoke(() => TargetDatamodels.Add(targetDatamodel));
                        }

                        return pingVector;
                    }).ToArray();
                    var currentVector = _pingCollectionVectorFactory.GetVector(vectors);
                    if (_previousVector != null)
                    {
                        //Log($"diff:{_vectorComparer.Compare(_previousVector, currentVector)}");
                    }

                    _previousVector = currentVector;
                    resortSubject.OnNext(0);
                });
            });

            TargetDatamodels = new ObservableCollection<TargetDatamodel>();
        }

        private IPingStats GetUpdatedStats(IPingResponse pingResponse)
        {
            PingStats stats;
            var targetIpAddress = pingResponse.TargetIpAddress;
            if (_stats.TryGetValue(targetIpAddress, out var found))
            {
                stats = found;
            }
            else
            {
                stats = new PingStats();
                _stats.Add(targetIpAddress, stats);
            }
            var isSuccess = _pingResponseUtil.IsSuccess(pingResponse.Status);
            _pingStatsUtil.AddStatus(stats.StatusHistory, isSuccess);
            var v = isSuccess ? 1.0 : 0.0;
            stats.Average25 = ((stats.Average25 * stats.Average25Count) + v) / (stats.Average25Count + 1);
            if (stats.Average25Count < 25)
            {
                stats.Average25Count++;
            }


            return stats;
        }

        private bool GetStatusSuccess(IPStatus pingResponseStatus)
        {
            switch (pingResponseStatus)
            {
                case IPStatus.Success:
                    return true;
                default:
                    return false;
            }
        }

        private void Log(string message)
        {
            Debug.WriteLine(message);
        }
    }

    public class PingStats : IPingStats
    {
        public IList<bool> StatusHistory { get; }
        public double Average25 { get; set; }
        public int Average25Count { get; set; }

        IEnumerable<bool> IPingStats.StatusHistory => StatusHistory;
        public PingStats()
        {
            StatusHistory = new List<bool>();
        }
    }

    public interface IPingStats
    {
        IEnumerable<bool> StatusHistory { get; }
        double Average25 { get; }
    }

    public class PingStatsUtil
    {
        private const int MaxHistoryCount = 55;

        public void AddStatus(IList<bool> statusHistory, bool status)
        {
            statusHistory.Add(status);
            if (statusHistory.Count > MaxHistoryCount)
            {
                statusHistory.RemoveAt(0);
            }
        }

        public Stats GetAverageSuccessRate(IEnumerable<bool> statusHistory)
        {
            var vals = statusHistory.Select(b => b ? 1.0 : 0).ToArray();
            var aggregate = vals.Aggregate(new Aggregates(), (s, val) =>
             {
                 if (!s.Min.HasValue || val < s.Min)
                 {
                     s.Min = val;
                 }
                 if (!s.Max.HasValue || val > s.Max)
                 {
                     s.Max = val;
                 }

                 s.Sum += val;
                 s.Count++;
                 return s;
             });
            var average = aggregate.Sum / aggregate.Count;
            Stats stats=new Stats { Average = average };
            const double tollerance = 0.001;
            if (Math.Abs((aggregate.Min??0) - (aggregate.Max??0)) < tollerance)
            {
                stats.StdDev = 0;
                stats.StdDevMinMax = 0;
            }
            else
            {
                stats = vals.Aggregate(stats, (s, val) =>
                {
                
                    bool isMinMax = false;
                    if (aggregate.Min.HasValue && Math.Abs(aggregate.Min.Value - val) < tollerance)
                    {
                        aggregate.Min = null;
                        isMinMax = true;
                    }
                    if (aggregate.Max.HasValue && Math.Abs(aggregate.Max.Value - val) < tollerance)
                    {
                        aggregate.Max = null;
                        isMinMax = true;
                    }

                    var diff = val - average;
                    var diffSquared = Math.Pow(diff, 2);
                    s.StdDev += diffSquared;
                    if (!isMinMax)
                    {
                        s.StdDevMinMax += diffSquared;
                    }

                    return s;
                });

                var stdDevFrac = stats.StdDev / aggregate.Count;
                var stdDev = Math.Sqrt(stdDevFrac);
                stats.StdDev = stdDev;

                var stdDevMMFrac = stats.StdDevMinMax / (aggregate.Count-2);
                var stdDevMM = Math.Sqrt(stdDevMMFrac);
                stats.StdDevMinMax = stdDevMM;
            }

            return stats;
        }
    }

    public class Aggregates
    {
        public double Count { get; set; }
        public double Sum { get; set; }
        public double? Min { get; set; }
        public double? Max { get; set; }
    }

    public class Stats
    {
        public double Average { get; set; }
        public double StdDev { get; set; }
        public double StdDevMinMax { get; set; }
    }
}