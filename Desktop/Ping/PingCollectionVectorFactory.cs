using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Desktop.Vector;
using zh.LocalPingLib.Ping;

namespace Desktop.Ping
{
    public class PingCollectionVectorFactory : IPingCollectionVectorFactory
    {
        private readonly IVectorInputConversionService<IPingResponse> _vectorInputConversionService;
        private IReadOnlyDictionary<IPAddress, IReadOnlyDictionary<IDimensionKey, Func<IPingResponse, Task<double>>>> _ipSpecificDimensions;

        public PingCollectionVectorFactory(IVectorInputConversionService<IPingResponse> vectorInputConversionService,
            IIpAddressService ipAddressService)
        {
            _vectorInputConversionService = vectorInputConversionService;

            Task<double> DimensionValueFactory(IPingResponse pr)
            {
                var status = pr.Status.GetHashCode();
                const double statusTrue = 104;
                const double statusFalse = 117;
                var statusFlag = status == 0 ? statusTrue : statusFalse;
                var dimValue = statusFlag;
                return Task.FromResult(dimValue);
            }

            ipAddressService.IpAddressObservable.Subscribe(a =>
            {
                _ipSpecificDimensions = a.ToDictionary(ip => ip, ip => (IReadOnlyDictionary<IDimensionKey, Func<IPingResponse, Task<double>>>)new Dictionary<IDimensionKey, Func<IPingResponse, Task<double>>>
                {
                    {new DimensionKey($"{ip} status flag"), DimensionValueFactory}
                });
            });
            
        }

        public async Task<IVector> GeVector(IEnumerable<IPingResponse> pingResponses)
        {
            var x = pingResponses.Select(async response =>
            {
                IReadOnlyDictionary<IDimensionKey, Func<IPingResponse, Task<double>>> dimensions;
                try
                {
                    
                    dimensions = _ipSpecificDimensions[response.TargetIpAddress];
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
                var vectorTask = _vectorInputConversionService.GetVector(response, dimensions);

                var vector = await vectorTask;
                var dimensionValues = vector.DimensionValues;
                return dimensionValues;
            });
            var y = await Task.WhenAll(x);
            var z = y.SelectMany(i=>i);
            return new Vector.Vector(z);
        }
    }
}