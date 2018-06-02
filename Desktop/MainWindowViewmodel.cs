using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Reactive.Linq;
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
        private readonly IDictionary<IPAddress, TargetDatamodel> _targetDatamodels;

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
            _targetDatamodels = new ConcurrentDictionary<IPAddress, TargetDatamodel>();
            var d = Dispatcher.CurrentDispatcher;
            
            ipAddressService.IpAddressObservable.Subscribe(ipAddresses =>
            {
                pingResponseSubscription?.Dispose();
                IObservable<IEnumerable<Task<IPingResponse>>> pingResponseObservable =
                    pingTimerObservable.Select(l => _pingService.Ping(ipAddresses));
                pingResponseSubscription = pingResponseObservable.Subscribe(async pingResponseTasks =>
                {
                    var tasks = pingResponseTasks.Select(async pingResponseTask =>
                    {
                        var pingResponse = await pingResponseTask;
                        TargetDatamodel targetDatamodel;
                        if (_targetDatamodels.TryGetValue(pingResponse.TargetIpAddress, out var targetDatamodelX))
                        {
                            targetDatamodel = targetDatamodelX;
                            targetDatamodel.RoundTripTime = pingResponse.RoundTripTime;
                            targetDatamodel.StatusSuccess = GetStatusSuccess(pingResponse.Status);
                        }
                        else
                        {
                            targetDatamodel = new TargetDatamodel(address: pingResponse.TargetIpAddress,
                                statusSuccess: GetStatusSuccess(pingResponse.Status),
                                roundTripTime: pingResponse.RoundTripTime);
                            _targetDatamodels.Add(pingResponse.TargetIpAddress, targetDatamodel);
                            d.Invoke(()=> TargetDatamodels.Add(targetDatamodel));
                        }
                        //Log($"address:{pingResponse.ReponseIpAddress} RTT:{pingResponse.RoundTripTime.TotalMilliseconds} status:{pingResponse.Status}");
                        return pingResponse;
                    });
                    var responses = await Task.WhenAll(tasks);
                    var vectors = responses.Select(_pingVectorFactory.GetVector);
                    var currentVector = _pingCollectionVectorFactory.GetVector(vectors);
                    if (_previousVector != null)
                    {
                        Log($"diff:{_vectorComparer.Compare(_previousVector, currentVector)}");
                    }

                    _previousVector = currentVector;
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
}