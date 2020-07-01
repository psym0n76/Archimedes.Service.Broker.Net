using Archimedes.Library.Message;
using EasyNetQ;

namespace Fx.MessageBus.Publishers
{
    public class NetQPublish : INetQPublish
    {
        private readonly string _host;

        public NetQPublish(string host)
        {
            _host = host;
        }

        public void PublishMessage(string message)
        {
            using (var bus = RabbitHutch.CreateBus(_host))
            {
                bus.Publish(message);
            }
        }

        public void PublishTradeMessage(ResponseTrade message)
        {
            using (var bus = RabbitHutch.CreateBus(_host))
            {
                bus.Publish(message);
            }
        }

        public void PublishCandleMessage(ResponseCandle message)
        {
            using (var bus = RabbitHutch.CreateBus(_host))
            {
                bus.Publish(message);
            }
        }

        public void PublishPriceMessage(ResponsePrice message)
        {
            using (var bus = RabbitHutch.CreateBus(_host))
            {
                bus.Publish(message);
            }
        }
    }
}