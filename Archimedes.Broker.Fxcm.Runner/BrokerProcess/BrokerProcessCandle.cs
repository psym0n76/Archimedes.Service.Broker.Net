using Archimedes.Library.Extensions;
using Archimedes.Library.Message;
using Archimedes.Library.Message.Dto;
using Archimedes.Library.RabbitMq;
using Fx.Broker.Fxcm;
using Fx.Broker.Fxcm.Models;
using NLog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Archimedes.Library.Logger;


namespace Archimedes.Broker.Fxcm.Runner
{
    public class BrokerProcessCandle : IBrokerProcessCandle
    {
        private readonly IProducerFanout<CandleMessage> _producer;
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly BatchLog _batchLog = new BatchLog();

        public BrokerProcessCandle(IProducerFanout<CandleMessage> producer)
        {
            _producer = producer;
        }

        public Task Run(CandleMessage message)
        {
            _batchLog.Start();
            var reconnect = 1;
            var session = BrokerSession.GetInstance();

            if (session.State == SessionState.Disconnected)
            {
                _batchLog.Update($"FXCM Connection status: {session.State}");
                _logger.Info($"FXCM Connection status: {session.State}");
                session.Connect();
            }

            while (session.State == SessionState.Reconnecting && reconnect < 10)
            {
                _batchLog.Update($"Waiting to reconnect for CandleRequest...{reconnect} Market: {message.Market} Timeframe: {message.TimeFrame} Interval: {message.Interval}");
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
                    _batchLog.Update($"FXCM Connection status: {session.State}");
                    _logger.Info($"FXCM Connection status: {session.State}");

                    GetCandleHistory(session, message);
                        
                    _producer.PublishMessage(message, "Archimedes_Candle");
                    _logger.Info(_batchLog.Print);
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

            _batchLog.Update($"FXCM Candles returned on Market: {request.Market} Granularity: {request.Interval}{request.TimeFrame} Count:{candles.Count}");
            //_logger.Info($"FXCM Items returned for Market: {request.Market} Granularity: {request.TimeFrame} Count:{candles.Count}");

            BuildResponse(request, candles);
        }

        private int GetBrokerOfferId(Session session, CandleMessage request)
        {

            var offers = session.GetOffers();

            if (offers == null)
            {
                _batchLog.Update($"Null returned from Offers: {request}");
                _logger.Warn($"{_batchLog.Print()}");

                return default;
            }

            // returns no offers
            var offer = offers.FirstOrDefault(o => o.Currency == request.Market);

            _batchLog.Update($"FXCM Candle Request {nameof(offer.OfferId)}: {offer.OfferId} {nameof(request.Market)}: {request.Market}  {nameof(request.TimeFrame)}{request.Interval}{request.TimeFrame} {nameof(request.StartDate)}: {request.StartDate} {nameof(request.EndDate)}: {request.EndDate}\n");

            //_logger.Info($"\n\n FXCM BrokerOffer " +
            //             $"\n  {nameof(offer.OfferId)}: {offer.OfferId} {nameof(request.Market)}: {request.Market} {nameof(request.Interval)}: {request.Interval} {nameof(request.TimeFrame)}: {request.TimeFrame}" +
            //             $"\n  {nameof(request.StartDate)}: {request.StartDate} {nameof(request.EndDate)}: {request.EndDate}\n");

            return offer.OfferId;
        }

        private static void BuildResponse(CandleMessage request, IEnumerable<Candle> candles)
        {
            var candleDto = candles.Select(c => new CandleDto()
            {
                TimeStamp = c.Timestamp,
                ToDate = c.Timestamp.AddMinutes(request.Interval),
                FromDate = c.Timestamp,
                BidOpen = decimal.Parse(c.BidOpen.ToString(CultureInfo.InvariantCulture)),
                BidHigh = decimal.Parse(c.BidHigh.ToString(CultureInfo.InvariantCulture)),
                BidLow = decimal.Parse(c.BidLow.ToString(CultureInfo.InvariantCulture)),
                BidClose = decimal.Parse(c.BidClose.ToString(CultureInfo.InvariantCulture)),
                AskOpen = decimal.Parse(c.AskOpen.ToString(CultureInfo.InvariantCulture)),
                AskHigh = decimal.Parse(c.AskLow.ToString(CultureInfo.InvariantCulture)),
                AskLow = decimal.Parse(c.AskLow.ToString(CultureInfo.InvariantCulture)),
                AskClose = decimal.Parse(c.AskClose.ToString(CultureInfo.InvariantCulture)),
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