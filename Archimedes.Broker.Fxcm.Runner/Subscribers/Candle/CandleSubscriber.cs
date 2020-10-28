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

        private void Consumer_HandleMessage(object sender, MessageHandlerEventArgs args)
        {
            var requestCandle = JsonConvert.DeserializeObject<CandleMessage>(args.Message);
            _logger.Info($"Received CandleRequest: {requestCandle}");

            try
            {
                Task.Run(() =>
                {
                    try
                    {
                        _brokerProcessCandle.Run(requestCandle);
                    }
                    catch (Exception e)
                    {
                        _logger.Error($"Error returned from BrokerProcessCandle: RequestCandle: {requestCandle}\n ERROR {e.Message} {e.StackTrace}");
                    }

                });

            }
            catch (Exception e)
            {
                _logger.Error($"Error returned from BrokerProcessCandle: RequestCandle: {requestCandle}\n ERROR {e.Message} {e.StackTrace}");
            }

        }

        public void SubscribeCandleMessage(Session session, CancellationToken cancellationToken)
        {
            _logger.Info($"Subscribed to CandleRequestQueue");
            _consumer.Subscribe(cancellationToken);
        }
    }
}