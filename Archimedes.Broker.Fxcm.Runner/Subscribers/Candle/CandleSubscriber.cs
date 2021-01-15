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
        private static readonly object LockingObject = new object();

        public CandleSubscriber(IBrokerProcessCandle brokerProcessCandle, ICandleConsumer consumer)
        {
            _brokerProcessCandle = brokerProcessCandle;
            _consumer = consumer;
            _consumer.HandleMessage += Consumer_HandleMessage;
        }

        private void Consumer_HandleMessage(object sender, CandleMessageHandlerEventArgs e)
        {
            _logId = _batchLog.Start($"Received CandleRequest {e.Message.Market} {e.Message.Interval} {e.Message.TimeFrame}");

            try
            {
                lock (LockingObject)
                {
                    _batchLog.Update(_logId, $"Processing Candle");
                    _brokerProcessCandle.CandleProcessor(e.Message);
                    _batchLog.Update(_logId, $"Processing Candle FINISHED");
                    _logger.Info(_batchLog.Print(_logId));
                }

            }
            catch (Exception ex)
            {
                _logger.Error(_batchLog.Print(_logId, "Error Returned from CandleSubscriber", ex));
            }
        }

        public void SubscribeCandleMessage(Session session, CancellationToken cancellationToken)
        {
            _logger.Info($"Subscribed to CandleRequestQueue");
            _consumer.Subscribe(cancellationToken, 100);
        }
    }
}