using System;
using System.Threading;
using Archimedes.Library.Logger;
using Fx.Broker.Fxcm;
using NLog;
using Archimedes.Library.RabbitMq;

namespace Archimedes.Broker.Fxcm.Runner
{
    public class PriceSubscriber : IPriceSubscriber
    {
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly IBrokerProcessPrice _brokerProcessPrice;
        private readonly IPriceConsumer _consumer;
        private readonly BatchLog _batchLog = new BatchLog();
        private string _logId;

        public PriceSubscriber(IBrokerProcessPrice brokerProcessPrice, IPriceConsumer consumer)
        {
            _brokerProcessPrice = brokerProcessPrice;
            _consumer = consumer;
            _consumer.HandleMessage += Consumer_HandleMessage;
        }

        private void Consumer_HandleMessage(object sender, PriceMessageHandlerEventArgs e)
        {
            _logId = _batchLog.Start();

            _batchLog.Update(_logId,
                $"Received PriceRequest {e.Message.Market}");
            
            try
            {
                _batchLog.Update(_logId, $"Processing Price BLOCKED TEMP");
                //_brokerProcessPrice.Run(e.Message);
                _batchLog.Update(_logId, $"Processing Price FINISHED");
                _logger.Info(_batchLog.Print(_logId));
            }
            catch (Exception ex)
            {
                _logger.Error(_batchLog.Print(_logId, "Error Returned from PriceSubscriber", ex));
            }
        }

        public void SubscribePriceMessage(Session session, CancellationToken cancellationToken)
        {
            _logger.Info($"Subscribed to PriceRequestQueue");
            _consumer.Subscribe(cancellationToken);
        }
    }
}