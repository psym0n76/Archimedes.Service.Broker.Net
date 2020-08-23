using Archimedes.Library.Message;

namespace Archimedes.Broker.Fxcm.Runner
{
    public interface IBrokerProcessTrade
    {
        void Run(TradeMessage request);
    }
}