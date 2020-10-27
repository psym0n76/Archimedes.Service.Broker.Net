using Archimedes.Library.Extensions;
using Archimedes.Library.Message;
using Archimedes.Library.Message.Dto;
using Fx.Broker.Fxcm;
using Fx.Broker.Fxcm.Models;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Archimedes.Library.RabbitMq;


// ReSharper disable once CheckNamespace
namespace Archimedes.Broker.Fxcm.Runner
{
    public class BrokerProcessCandle : IBrokerProcessCandle
    {
        private readonly IProducer<CandleMessage> _producer;
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public BrokerProcessCandle(IProducer<CandleMessage> producer)
        {
            _producer = producer;
        }

        public Task Run(CandleMessage message)
        {
            var reconnect = 1;
            var session = BrokerSession.GetInstance();

            _logger.Info($"FXCM Connection status: {session.State} Market: {message.Market} Timeframe: {message.TimeFrame} Interval: {message.Interval}");

            if (session.State == SessionState.Disconnected)
            {
                session.Connect();
            }

            while (session.State == SessionState.Reconnecting && reconnect < 10)
            {
                _logger.Info($"Waiting to reconnect for CandleRequest...{reconnect} Market: {message.Market} Timeframe: {message.TimeFrame} Interval: {message.Interval}");
                reconnect++;
                Thread.Sleep(5000);
            }

            switch (session.State)
            {
                case SessionState.Disconnected:
                    return Task.FromException<long>(
                        new ApplicationException($"Unable to connect to FXCM: {session.State}"));

                case SessionState.Connected:
                    _logger.Info($"FXCM Connection status: {session.State}");

                    try
                    {
                        GetCandleHistory(session, message);
                        _producer.PublishMessage(message, "CandleResponseQueue");
                    }

                    catch (Exception e)
                    {
                        return Task.FromException<long>(
                            new ApplicationException($"Candle History: FXCM Connection Failed: {e.Message}"));
                    }

                    break;

                case SessionState.Reconnecting:
                    return Task.FromException<long>(
                        new ApplicationException($"Candle History: FXCM Reconnection limit hit : {reconnect}"));
 
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return Task.CompletedTask;
        }
    


        private void GetCandleHistory(Session session, CandleMessage request)
        {
            var offerId = GetBrokerOfferId(session, request);

            request.CountCandleIntervals();

            var candles = session.GetCandles(offerId, request.TimeFrameBroker, request.Intervals,
                request.StartDate, request.EndDate);

            _logger.Info($"FXCM Items returned for Market: {request.Market} Granularity: {request.TimeFrame} Count:{candles.Count}");


            BuildResponse(request, candles);
        }

        private int GetBrokerOfferId(Session session, CandleMessage request)
        {
            request.Logs.Add("Candle Response from Broker");

            var offers = session.GetOffers();

            if (offers == null)
            {
                _logger.Warn($"Null returned from Offers: {request}");
                request.Logs.Add($"Null returned from Offers: {request}");
                return default;
            }

            // returns no offers
            var offer = offers.FirstOrDefault(o => o.Currency == request.Market);

            _logger.Info($"\n\n FXCM BrokerOffer " +
                         $"\n  {nameof(offer.OfferId)}: {offer.OfferId} {nameof(request.Market)}: {request.Market} {nameof(request.Interval)}: {request.Interval} {nameof(request.TimeFrame)}: {request.TimeFrame}" +
                         $"\n  {nameof(request.StartDate)}: {request.StartDate} {nameof(request.EndDate)}: {request.EndDate}\n");

            return offer.OfferId;
        }

        private static void BuildResponse(CandleMessage request, IEnumerable<Candle> candles)
        {
            var candleDto = candles.Select(c => new CandleDto()
                {
                    TimeStamp = c.Timestamp,
                    ToDate = c.Timestamp.AddMinutes(request.Interval),
                    FromDate = c.Timestamp,
                    BidOpen = c.BidOpen,
                    BidHigh = c.BidHigh,
                    BidLow = c.BidLow,
                    BidClose = c.BidClose,
                    AskOpen = c.AskOpen,
                    AskHigh = c.AskHigh,
                    AskLow = c.AskLow,
                    AskClose = c.AskClose,
                    TickQty = c.TickQty,
                    Market = request.Market,
                    MarketId = request.MarketId,
                    Granularity = $"{request.Interval}{request.TimeFrame}",
                    LastUpdated = DateTime.Now
                })
                .ToList();

            request.Candles = candleDto;
            request.Success = true;
        }
    }
}