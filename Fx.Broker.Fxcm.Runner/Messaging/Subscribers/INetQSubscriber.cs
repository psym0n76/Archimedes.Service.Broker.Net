namespace FXCMRestRunner
{
    public interface INetQSubscriber
    {
        void SubscribeCandleMessage();
        void SubscribeTradeMessage();
    }
}