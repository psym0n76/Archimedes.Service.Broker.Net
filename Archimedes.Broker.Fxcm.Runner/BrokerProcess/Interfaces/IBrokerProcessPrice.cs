using Archimedes.Library.Message;

namespace Archimedes.Broker.Fxcm.Runner
{
    public interface IBrokerProcessPrice
    {
        void PriceProcessor(PriceMessage request);
    }
}