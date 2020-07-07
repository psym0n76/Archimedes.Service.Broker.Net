using Archimedes.Library.Message;
using Archimedes.Library.Message.Dto;
using NLog;
using System.Collections.Generic;
using System.Threading.Tasks;
using Archimedes.Library.EasyNetQ;

namespace Archimedes.Broker.Fxcm.Runner
{
    public class BrokerProcessPriceTest : IBrokerProcessPrice
    {
        private readonly INetQPublish<ResponsePrice> _netQPublish;
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public BrokerProcessPriceTest(INetQPublish<ResponsePrice> netQPublish)
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

                _netQPublish.PublishMessage(priceResponse);

            }).ConfigureAwait(false);
        }
    }
}