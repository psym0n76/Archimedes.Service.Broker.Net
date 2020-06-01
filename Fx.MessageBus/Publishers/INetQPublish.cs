using Archimedes.Library.Message;

namespace Fx.MessageBus.Publishers
{
    public interface INetQPublish
    {
        void PublishMessage(string message);
        void PublishTradeMessage(ResponseTrade message);
        void PublishCandleMessage(ResponseCandle message);
        void PublishPriceMessage(ResponsePrice message);

        
    }
}