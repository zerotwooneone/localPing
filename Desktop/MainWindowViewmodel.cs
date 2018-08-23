using Desktop.Dispatcher;
using Desktop.Target;
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
using zh.LocalPingLib.Ping;
using zh.Vector;

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
            IPingResponseUtil pingResponseUtil,
            IDimensionKeyFactory dimensionKeyFactory)
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
            System.Windows.Threading.Dispatcher d = dispatcherAccessor.GetDispatcher();
            Subject<int> resortSubject = new Subject<int>();
            ResortObservable = resortSubject;

            ipAddressService.IpAddressObservable.Subscribe(ipAddresses =>
            {
                pingResponseSubscription?.Dispose();
                IObservable<IEnumerable<Task<IPingResponse>>> pingResponseObservable =
                    pingTimerObservable.Select(l => _pingService.Ping(ipAddresses));
                pingResponseSubscription = pingResponseObservable.Subscribe(async pingResponseTasks =>
                {
                    IPingResponse[] responses = await Task.WhenAll(pingResponseTasks);
                    IVector[] vectors = responses.Select(pingResponse =>
                    {
                        IPingStats stats = GetUpdatedStats(pingResponse);
                        IVector pingVector = _pingVectorFactory.GetVector(pingResponse, stats);
                        if (_targetDatamodels.TryGetValue(pingResponse.TargetIpAddress, out PingState pingState))
                        {
                            TargetDatamodel targetDatamodelX = pingState.TargetDatamodel;
                            targetDatamodelX.RoundTripTime = pingResponse.RoundTripTime;
                            targetDatamodelX.StatusSuccess = GetStatusSuccess(pingResponse.Status);
                            var boring = _pingVectorFactory.GetVector(new PingResponse(IPAddress.Loopback, TimeSpan.Zero, IPStatus.DestinationHostUnreachable, IPAddress.Loopback),
                                new PingStats { Average25 = 0, Average25Count = 25, StatusHistory = new bool[PingStatsUtil.MaxHistoryCount] });
                            double change = _vectorComparer.Compare(boring, pingVector);
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
                            TargetDatamodel targetDatamodel = new TargetDatamodel(address: pingResponse.TargetIpAddress,
                                statusSuccess: GetStatusSuccess(pingResponse.Status),
                                roundTripTime: pingResponse.RoundTripTime);
                            _targetDatamodels.Add(pingResponse.TargetIpAddress, new PingState { TargetDatamodel = targetDatamodel, Previous = pingVector });
                            d.Invoke(() => TargetDatamodels.Add(targetDatamodel));
                        }

                        return pingVector;
                    }).ToArray();
                    IVector currentVector = _pingCollectionVectorFactory.GetVector(vectors);
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
            IPAddress targetIpAddress = pingResponse.TargetIpAddress;
            if (_stats.TryGetValue(targetIpAddress, out PingStats found))
            {
                stats = found;
            }
            else
            {
                stats = new PingStats();
                _stats.Add(targetIpAddress, stats);
            }
            bool isSuccess = _pingResponseUtil.IsSuccess(pingResponse.Status);
            _pingStatsUtil.AddStatus(stats.StatusHistory, isSuccess);
            double v = isSuccess ? 1.0 : 0.0;
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
}