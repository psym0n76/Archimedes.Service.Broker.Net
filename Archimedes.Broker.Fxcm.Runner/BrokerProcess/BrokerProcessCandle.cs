using Archimedes.Library.Extensions;
using Archimedes.Library.Message;
using Archimedes.Library.Message.Dto;
using Fx.Broker.Fxcm;
using Fx.Broker.Fxcm.Models;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Archimedes.Library.EasyNetQ;


// ReSharper disable once CheckNamespace
namespace Archimedes.Broker.Fxcm.Runner
{
    public class BrokerProcessCandle : IBrokerProcessCandle
    {
        private readonly INetQPublish<ResponseCandle> _netQPublish;
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public BrokerProcessCandle(INetQPublish<ResponseCandle> netQPublish)
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

                if (session.State == SessionState.Disconnected)
                {
                    _logger.Error($"Unable to connect to FXCM");
                    return;
                }

                _logger.Info($"Process Candle History: {request}");

                _netQPublish.PublishMessage(CandleHistory(session, request));

            }).ConfigureAwait(false);
        }


        private ResponseCandle CandleHistory(Session session, RequestCandle request)
        {
            var response = new ResponseCandle
                {Text = "Candle Response from Broker", Payload = new List<CandleDto>(), Status = "Live",Request = request};

            var offers = session.GetOffers();
            var offer = offers.FirstOrDefault(o => o.Currency == request.Market);


            if (!ValidateRequest(request, offer, response)) 
                return response;

            var candles = session.GetCandles(offer.OfferId, request.TimeFrameInterval, 1,
                request.StartDate.BrokerDate(), request.EndDate.BrokerDate());


            return BuildResponse(request, candles, response);
        }

        private ResponseCandle BuildResponse(RequestCandle request, IList<Candle> candles, ResponseCandle response)
        {
            if (candles == null)
            {
                const string message = "Candle empty";
                _logger.Info(message);
                response.Status = message;
                return response;
            }

            var candleDto = candles.Select(c => new CandleDto()
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
                    TickQty = c.TickQty,
                    Market = request.Market
                })
                .ToList();

            response.Payload = candleDto;

            return response;
        }

        private bool ValidateRequest(RequestCandle request, Offer offer, ResponseCandle response)
        {
            if (offer == null)
            {
                var message = $"The instrument {request.Market} is not valid";
                _logger.Info(message);
                response.Status = message;
                return false;
            }

            if (request.StartDate.BrokerDate() != DateTime.MinValue &&
                request.EndDate.BrokerDate() == DateTime.MinValue)
            {
                var message = $"Incorrect Date formats {request.StartDate.BrokerDate()} {request.EndDate.BrokerDate()}";
                _logger.Info(message);
                response.Status = message;
                return false;
            }

            return true;
        }
    }
}