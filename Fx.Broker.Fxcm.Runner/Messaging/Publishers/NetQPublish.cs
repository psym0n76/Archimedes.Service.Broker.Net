using EasyNetQ;
using EasyNetQMessageDotNet;

namespace FXCMRestRunner
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

        public void PublishMessageClass<T>(T message) where T : IActionMessage
        {
            using (var bus = RabbitHutch.CreateBus(Host))
            {
                bus.Publish<IActionMessage>(message);
            }
        }
    }
}