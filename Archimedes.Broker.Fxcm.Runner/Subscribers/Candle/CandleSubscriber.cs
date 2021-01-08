using System;
using System.Threading;
using Archimedes.Library.Logger;
using Fx.Broker.Fxcm;
using NLog;
using Archimedes.Library.RabbitMq;

namespace Archimedes.Broker.Fxcm.Runner
{
    public class CandleSubscriber : ICandleSubscriber
    {
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly IBrokerProcessCandle _brokerProcessCandle;
        private readonly ICandleConsumer _consumer;
        private readonly BatchLog _batchLog = new BatchLog();
        private string _logId;

        public CandleSubscriber(IBrokerProcessCandle brokerProcessCandle, ICandleConsumer consumer)
        {
            _brokerProcessCandle = brokerProcessCandle;
            _consumer = consumer;
            _consumer.HandleMessage += Consumer_HandleMessage;
        }

        private async void Consumer_HandleMessage(object sender, CandleMessageHandlerEventArgs e)
        {
            _logId = _batchLog.Start();

            _batchLog.Update(_logId, $"Received CandleRequest: {e.Message.Market} {e.Message.Interval}{e.Message.TimeFrame}");

            try
            {
                await _brokerProcessCandle.Run(e.Message);
                _batchLog.Update(_logId, $"Finished Processing Candle");
                _logger.Info(_batchLog.Print(_logId));
                //Task.Run(() => { _brokerProcessCandle.Run(requestCandle); });
            }
            catch (Exception ex)
            {
                _logger.Error(
                    $"Error returned from BrokerProcessCandle: RequestCandle: {e.Message}\n ERROR {ex.Message} {ex.StackTrace}");
            }
        }

        public void SubscribeCandleMessage(Session session, CancellationToken cancellationToken)
        {
            _logger.Info($"Subscribed to CandleRequestQueue");
            _consumer.Subscribe(cancellationToken, 100);
        }
    }
}