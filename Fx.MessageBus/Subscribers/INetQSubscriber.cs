namespace Fx.MessageBus.Subscribers
{
    public interface INetQSubscriber
    {
        void SubscribeCandleMessage();
        void SubscribeTradeMessage();
    }
}