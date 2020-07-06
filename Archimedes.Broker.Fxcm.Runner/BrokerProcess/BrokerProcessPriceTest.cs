using System.Collections.Generic;
using System.Threading.Tasks;
using Archimedes.Library.Message;
using Archimedes.Library.Message.Dto;
using Fx.MessageBus.Publishers;
using NLog;

namespace Archimedes.Broker.Fxcm.Runner
{
    public class BrokerProcessPriceTest : IBrokerProcessPrice
    {
        private readonly INetQPublish _netQPublish;
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public BrokerProcessPriceTest(INetQPublish netQPublish)
        {
            _netQPublish = netQPublish;
        }

        public void Run(RequestPrice request)
        {
            Task.Run(() =>
            {

                _logger.Info("Process Price Update");

                var priceResponse = new ResponsePrice()
                {
                    Payload = new List<PriceDto>()
                    {
                        new PriceDto()
                        {
                            BidOpen = 1.2587,
                            BidClose = 1.2547,

                            AskOpen = 1.3569,
                            AskClose = 1.3570,

                            Market = request.Market
                        }
                    }
                };

                _netQPublish.PublishPriceMessage(priceResponse);

            }).ConfigureAwait(false);
        }
    }
}