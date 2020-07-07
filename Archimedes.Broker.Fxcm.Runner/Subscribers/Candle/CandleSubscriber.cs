using System.Configuration;
using System.Threading;
using System.Threading.Tasks;
using Archimedes.Library.Message;
using EasyNetQ;
using Fx.Broker.Fxcm;
using NLog;

namespace Archimedes.Broker.Fxcm.Runner
{
    public class CandleSubscriber : ICandleSubscriber
    {
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly IBrokerProcessCandle _brokerProcessCandle;

        public CandleSubscriber(IBrokerProcessCandle brokerProcessCandle)
        {
            _brokerProcessCandle = brokerProcessCandle;
        }

        public void SubscribeCandleMessage(Session session)
        {
            Task.Run(() =>
            {
                using (var bus = RabbitHutch.CreateBus(ConfigurationManager.AppSettings["RabbitHutchConnection"]))
                {
                    bus.Subscribe<RequestCandle>("Candle", @interface =>
                    {
                        if (@interface is RequestCandle candle)
                        {
                            _logger.Info($"Candle Message Recieved: {candle.Text}");

                            _brokerProcessCandle.Run(candle);
                        }
                    });

                    _logger.Info("Listening for Candle messages");

                    while (true)
                    {
                        Thread.Sleep(1000);

                    }
                }
            });
        }
    }
}