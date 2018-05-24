using System;
using Desktop.Vector;
using zh.LocalPingLib.Ping;

namespace Desktop.Ping
{
    public class PerResponseDimensionValueFactory
    {
        private readonly Func<IPingResponse, IDimensionKey> _dimensionKeyFactory;
        private readonly Func<IPingResponse, double> _dimensionValueFactory;

        public PerResponseDimensionValueFactory(Func<IPingResponse, IDimensionKey> dimensionKeyFactory,
            Func<IPingResponse, double> dimensionValueFactory)
        {
            _dimensionKeyFactory = dimensionKeyFactory;
            _dimensionValueFactory = dimensionValueFactory;
        }

        public IDimensionValue GetDimensionValue(IPingResponse pingResponse)
        {
            var dimensionKey = _dimensionKeyFactory(pingResponse);
            var dimensionValue = _dimensionValueFactory(pingResponse);
            return new DimensionValue(dimensionKey, dimensionValue);
        }
    }
}