﻿using System;
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
            IIpAddressService ipAddressService,
            IDimensionKeyFactory dimensionKeyFactory)
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

            ipAddressService.IpAddressObservable.Subscribe(ipAddresses =>
            {
                _ipSpecificDimensions = ipAddresses.ToDictionary(ip => ip, ip => (IReadOnlyDictionary<IDimensionKey, Func<IPingResponse, Task<double>>>)new Dictionary<IDimensionKey, Func<IPingResponse, Task<double>>>
                {
                    {dimensionKeyFactory.GetOrCreate($"{ip} status flag"), DimensionValueFactory}
                });
            });

        }

        public async Task<IVector> GeVector(IEnumerable<IPingResponse> pingResponses)
        {
            var dimensionValueTasks = pingResponses.Select(async response =>
            {
                var dimensions = _ipSpecificDimensions[response.TargetIpAddress];
                var vectorTask = _vectorInputConversionService.GetVector(response, dimensions);

                var vector = await vectorTask;
                var vectorValues = vector.DimensionValues;
                return vectorValues;
            });
            var dimensionValueLists = await Task.WhenAll(dimensionValueTasks);
            var dimensionValues = dimensionValueLists.SelectMany(i => i);
            return new Vector.Vector(dimensionValues);
        }
    }
}