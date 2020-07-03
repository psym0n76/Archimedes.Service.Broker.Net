using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Archimedes.Library.Message;
using Archimedes.Library.Message.Dto;
using Fx.Broker.Fxcm;
using Fx.MessageBus.Publishers;
using NLog;

namespace Archimedes.Broker.Fxcm.Runner
{
    public class BrokerProcessPrice : IBrokerProcessPrice
    {
        private readonly INetQPublish _netQPublish;
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public BrokerProcessPrice(INetQPublish netQPublish)
        {
            _netQPublish = netQPublish;
        }

        public void Run(RequestPrice request)
        {
            Task.Run(() =>
            {
                var session = BrokerSession.GetInstance();

                _logger.Info("Process Price Update");

                if (session.State == SessionState.Disconnected)
                {
                    session.Connect();
                }

                session.SubscribeSymbol(request.Market);
                //session.PriceUpdate += Session_PriceUpdate;

                session.PriceUpdate += priceUpdate =>
                {
                    var priceResponse = new ResponsePrice()
                    {
                        Payload = new List<PriceDto>()
                        {
                            new PriceDto()
                            {
                                BidOpen = priceUpdate.Bid,
                                BidClose = priceUpdate.Bid,

                                AskOpen = priceUpdate.Ask,
                                AskClose = priceUpdate.Ask,

                                Market = request.Market
                            }
                        }
                    };

                    _netQPublish.PublishPriceMessage(priceResponse);
                };

                Console.ReadLine();

                //session.PriceUpdate -= Session_PriceUpdate;
                session.UnsubscribeSymbol(request.Market);
            }).ConfigureAwait(false);
        }
    }
}