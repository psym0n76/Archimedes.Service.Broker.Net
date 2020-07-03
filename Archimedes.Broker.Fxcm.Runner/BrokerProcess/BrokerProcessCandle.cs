using Fx.Broker.Fxcm.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Archimedes.Library.Extensions;
using Archimedes.Library.Message;
using Archimedes.Library.Message.Dto;
using Fx.Broker.Fxcm;
using Fx.MessageBus.Publishers;
using NLog;


// ReSharper disable once CheckNamespace
namespace Archimedes.Broker.Fxcm.Runner
{
    public class BrokerProcessCandle : IBrokerProcessCandle
    {
        private readonly INetQPublish _netQPublish;
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        
        public BrokerProcessCandle(INetQPublish netQPublish)
        {
            _netQPublish = netQPublish;
        }

        public void Run(RequestCandle request)
        {
            Task.Run(() =>
            {
                var session = BrokerSession.GetInstance();

                if (session.State == SessionState.Disconnected)
                {
                    session.Connect();
                }

                _logger.Info($"Process Candle History: {request}");

                var candleHistory = GetHistory(session, request);

                var candleHistoryDto = GetCandleHistoryDto(candleHistory);

                _netQPublish.PublishCandleMessage(candleHistoryDto);

            }).ConfigureAwait(false);
        }


        private IList<Candle> GetHistory(Session session, RequestCandle request)
        {

            var offers = session.GetOffers();
            var offer = offers.FirstOrDefault(o => o.Currency == request.Market);
            if (offer == null)
            {
                _logger.Info($"The instrument {request.Market} is not valid");
                return null;
            }

            if (request.StartDate.BrokerDate() != DateTime.MinValue && request.EndDate.BrokerDate() == DateTime.MinValue)
            {
                _logger.Info($"Please provide DateTo in configuration file");
                return null;
            }

            const int unknownCount = 1;

            return session.GetCandles(offer.OfferId, request.TimeFrameInterval, unknownCount,
                request.StartDate.BrokerDate(), request.EndDate.BrokerDate());
        }

        private ResponseCandle GetCandleHistoryDto(IList<Candle> candle)
        {
            if (candle == null)
            {
                _logger.Info($"Candle empty");
                return null;
            }

            var result = new ResponseCandle {Text = "something", Payload = new List<CandleDto>()};

            var candleDto = candle.Select(c => new CandleDto()
                {
                    Timestamp = c.Timestamp,
                    BidOpen = c.BidOpen,
                    BidHigh = c.BidHigh,
                    BidLow = c.BidLow,
                    BidClose = c.BidClose,
                    AskOpen = c.AskOpen,
                    AskHigh = c.AskHigh,
                    AskLow = c.AskLow,
                    AskClose = c.AskClose,
                    TickQty = c.TickQty
                })
                .ToList();

            result.Payload = candleDto;

            return result;
        }
    }
}