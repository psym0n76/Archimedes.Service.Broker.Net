using Archimedes.Library.Message;
using EasyNetQ;

namespace Fx.MessageBus.Publishers
{
    public class NetQPublish : INetQPublish
    {
        private const string Host = "host=localhost";

        public void PublishMessage(string message)
        {
            using (var bus = RabbitHutch.CreateBus(Host))
            {
                bus.Publish(message);
            }
        }

        public void PublishTradeMessage(ResponseTrade message)
        {
            using (var bus = RabbitHutch.CreateBus(Host))
            {

                bus.Publish(message);
            }
        }

        public void PublishCandleMessage(ResponseCandle message)
        {
            using (var bus = RabbitHutch.CreateBus(Host))
            {

                bus.Publish(message);
            }
        }

        public void PublishPriceMessage(ResponsePrice message)
        {
            using (var bus = RabbitHutch.CreateBus(Host))
            {

                bus.Publish<ResponsePrice>(message);
            }
        }
    }
}