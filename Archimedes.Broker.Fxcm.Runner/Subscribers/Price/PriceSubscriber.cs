using Archimedes.Library.Message;
using EasyNetQ;
using Fx.Broker.Fxcm;
using NLog;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;

namespace Archimedes.Broker.Fxcm.Runner
{
    public class PriceSubscriber : IPriceSubscriber
    {
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly IBrokerProcessPrice _brokerProcessPrice;

        public PriceSubscriber(IBrokerProcessPrice brokerProcessPrice)
        {
            _brokerProcessPrice = brokerProcessPrice;
        }

        public void SubscribePriceMessage(Session session)
        {
            Task.Run(() =>
            {
                using (var bus = RabbitHutch.CreateBus(ConfigurationManager.AppSettings["RabbitHutchConnection"]))
                {
                    bus.Subscribe<RequestPrice>("Price", @interface =>
                    {
                        if (@interface is RequestPrice price)
                        {
                            _logger.Info($"Price Message Recieved: {price.Text}");

                            _brokerProcessPrice.Run(price);
                        }
                    });

                    _logger.Info("Listening for Price messages");

                    while (true)
                    {
                        Thread.Sleep(1000);
                    }
                }
            });
        }
    }
}