using Archimedes.Library.Message;

namespace Archimedes.Broker.Fxcm.Runner
{
    public interface IBrokerProcessCandle
    {
        void Run(CandleMessage message);
    }
}