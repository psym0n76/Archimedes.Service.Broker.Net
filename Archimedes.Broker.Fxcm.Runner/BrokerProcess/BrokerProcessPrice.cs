using Archimedes.Library.Message;
using Archimedes.Library.Message.Dto;
using Fx.Broker.Fxcm;
using NLog;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Archimedes.Library.EasyNetQ;

namespace Archimedes.Broker.Fxcm.Runner
{
    public class BrokerProcessPrice : IBrokerProcessPrice
    {
        private readonly INetQPublish<ResponsePrice> _netQPublish;
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public BrokerProcessPrice(INetQPublish<ResponsePrice> netQPublish)
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

                    _netQPublish.PublishMessage(priceResponse);
                };

                Console.ReadLine();

                //session.PriceUpdate -= Session_PriceUpdate;
                session.UnsubscribeSymbol(request.Market);
            }).ConfigureAwait(false);
        }
    }
}