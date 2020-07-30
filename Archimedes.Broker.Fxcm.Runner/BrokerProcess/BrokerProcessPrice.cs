using Archimedes.Library.Message;
using Archimedes.Library.Message.Dto;
using Fx.Broker.Fxcm;
using NLog;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Archimedes.Library.EasyNetQ;
using Microsoft.Extensions.Primitives;

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

                if (session.State == SessionState.Disconnected)
                {
                    _logger.Error("Unalbe to connect to FCXM");
                    return;
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

                while (true)
                {
                    //loop tpo keep session open
                }

                //session.PriceUpdate -= Session_PriceUpdate;
                session.UnsubscribeSymbol(request.Market);
            }).ConfigureAwait(false);
        }
    }
}