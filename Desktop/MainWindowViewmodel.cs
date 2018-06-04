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
        private IVector _previousVector;

        public ObservableCollection<TargetDatamodel> TargetDatamodels { get; }
        private readonly IDictionary<IPAddress, PingState> _targetDatamodels;
        public IObservable<int> ResortObservable { get; }

        public MainWindowViewmodel(IPingTimer pingTimer,
            IPingService pingService,
            IPingCollectionVectorFactory pingCollectionVectorFactory,
            IPingVectorFactory pingVectorFactory,
            IVectorComparer vectorComparer,
            IIpAddressService ipAddressService)
        {
            _pingTimer = pingTimer;
            _pingService = pingService;
            _pingCollectionVectorFactory = pingCollectionVectorFactory;
            _pingVectorFactory = pingVectorFactory;
            _vectorComparer = vectorComparer;
            IObservable<long> pingTimerObservable = _pingTimer.Start(() => false);
            IDisposable pingResponseSubscription = null;
            _targetDatamodels = new ConcurrentDictionary<IPAddress, PingState>();
            var d = Dispatcher.CurrentDispatcher;
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
                        var v = _pingVectorFactory.GetVector(pingResponse);
                        if (_targetDatamodels.TryGetValue(pingResponse.TargetIpAddress, out var pingState))
                        {
                            var targetDatamodelX = pingState.TargetDatamodel;
                            targetDatamodelX.RoundTripTime = pingResponse.RoundTripTime;
                            targetDatamodelX.StatusSuccess = GetStatusSuccess(pingResponse.Status);
                            var change = _vectorComparer.Compare(pingState.Previous, v);
                            targetDatamodelX.Change = change;

                            pingState.Previous = v;
                        }
                        else
                        {
                            var targetDatamodel = new TargetDatamodel(address: pingResponse.TargetIpAddress,
                                statusSuccess: GetStatusSuccess(pingResponse.Status),
                                roundTripTime: pingResponse.RoundTripTime);
                            _targetDatamodels.Add(pingResponse.TargetIpAddress, new PingState{TargetDatamodel = targetDatamodel, Previous = v});
                            d.Invoke(()=> TargetDatamodels.Add(targetDatamodel));
                        }

                        return v;
                    });
                    var currentVector = _pingCollectionVectorFactory.GetVector(vectors);
                    if (_previousVector != null)
                    {
                        Log($"diff:{_vectorComparer.Compare(_previousVector, currentVector)}");
                    }

                    _previousVector = currentVector;
                    resortSubject.OnNext(0);
                });
            });

            TargetDatamodels = new ObservableCollection<TargetDatamodel>();
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

    public class PingState
    {
        public TargetDatamodel TargetDatamodel { get; set; }
        public IVector Previous { get; set; }
    }
}