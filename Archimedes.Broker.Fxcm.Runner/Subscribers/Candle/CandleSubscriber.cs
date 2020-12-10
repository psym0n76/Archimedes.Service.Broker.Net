using System;
using System.Threading;
using System.Threading.Tasks;
using Archimedes.Library.Message;
using Fx.Broker.Fxcm;
using NLog;
using Archimedes.Library.RabbitMq;
using Newtonsoft.Json;

namespace Archimedes.Broker.Fxcm.Runner
{
    public class CandleSubscriber : ICandleSubscriber
    {
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly IBrokerProcessCandle _brokerProcessCandle;
        private readonly ICandleConsumer _consumer;

        public CandleSubscriber(IBrokerProcessCandle brokerProcessCandle, ICandleConsumer consumer)
        {
            _brokerProcessCandle = brokerProcessCandle;
            _consumer = consumer;
            _consumer.HandleMessage += Consumer_HandleMessage;
        }

        private void Consumer_HandleMessage(object sender, CandleMessageHandlerEventArgs e)
        {
            var requestCandle = JsonConvert.DeserializeObject<CandleMessage>(e.Message);
            _logger.Info($"Received CandleRequest: {requestCandle}");

            try
            {
                Task.Run(() => { _brokerProcessCandle.Run(requestCandle); });
            }
            catch (Exception ex)
            {
                _logger.Error(
                    $"Error returned from BrokerProcessCandle: RequestCandle: {requestCandle}\n ERROR {ex.Message} {ex.StackTrace}");
            }
        }

        public void SubscribeCandleMessage(Session session, CancellationToken cancellationToken)
        {
            _logger.Info($"Subscribed to CandleRequestQueue");
            _consumer.Subscribe(cancellationToken);
        }
    }
}