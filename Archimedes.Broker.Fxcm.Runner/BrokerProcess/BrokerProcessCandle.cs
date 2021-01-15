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
        private readonly IProducerFanout<CandleMessage> _producerFanout;
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly BatchLog _batchLog = new BatchLog();
        private string _logId;
        private const int RetryMax = 30;
        private const int Timeout = 5;

        public BrokerProcessCandle(IProducerFanout<CandleMessage> producerFanout)
        {
            _producerFanout = producerFanout;
        }

        public Task Run(CandleMessage request)
        {
            _logId = _batchLog.Start();
            _batchLog.Update(_logId, $"CandleRequest: {request.Id}");
            _batchLog.Update(_logId,
                $"CandleRequest: {request.Market} {request.Interval}{request.TimeFrame} {request.StartDate} to {request.EndDate}");

            if (request.StartDate > request.EndDate)
            {
                _logger.Warn(_batchLog.Print(_logId, $"Start Date greater then EndDate"));
                return default;
            }

            var retry = 1;
            var session = BrokerSession.GetInstance();

            _batchLog.Update(_logId, $"Instance {request.Market} for Candles");

            if (session.State == SessionState.Disconnected)
            {
                _batchLog.Update(_logId, $"Connection status: {session.State}");
                session.Connect();
            }

            while (session.State == SessionState.Reconnecting && retry < RetryMax)
            {
                _batchLog.Update(_logId, $"Waiting to Connect: {session.State} elapsed {retry * Timeout} Sec(s)");
                Thread.Sleep(Timeout * 1000);
                retry++;
            }


            switch (session.State)
            {
                case SessionState.Disconnected:

                    _logger.Error(_batchLog.Print(_logId, $"Unable to connect: {session.State}"));
                    break;

                case SessionState.Connected:

                    _batchLog.Update(_logId, $"Connection status: {session.State}");

                    if (CandleHistory(session, request))
                    {
                        PublishCandles(request);
                        _logger.Info(_batchLog.Print(_logId));
                    }

                    break;

                case SessionState.Reconnecting:

                    _logger.Error(_batchLog.Print(_logId,
                        $"Reconnection limit hit : {retry}"));
                    break;


                default:
                    _logger.Error(_batchLog.Print(_logId, $"Unknown SessionState : {session.State}"));
                    break;

            }

            return default;
        }

        private void PublishCandles(CandleMessage message)
        {
            // we need to publish to both the individual candle queues for the trader service but also publish to the group for repo service
            _producerFanout.PublishMessage(message, message.QueueName);
            _producerFanout.PublishMessage(message, "Archimedes_Candle");
            _batchLog.Update(_logId,
                $"Publish to {message.QueueName} {message.Market} {message.Interval}{message.TimeFrame}");
        }

        private bool CandleHistory(Session session, CandleMessage request)
        {
            try
            {
                request.CountCandleIntervals();

                var offers = session.GetOffers();

                var offer = offers.FirstOrDefault(o => o.Currency == request.Market);

                _batchLog.Update(_logId, $"OfferId for {request.Market} {offer.OfferId}");

                var candles = session.GetCandles(request.ExternalMarketId, request.TimeFrameBroker, request.Intervals,
                    request.StartDate, request.EndDate);

                if (candles == null)
                {
                    _batchLog.Update(_logId,
                        $"Candle Response: Candle object is null {candles}");
                    return false;
                }

                _batchLog.Update(_logId,
                    $"Candle Response: {candles.Count} Candle(s)");

                BuildResponse(request, candles);

                return true;
            }
            catch (Exception e)
            {
                _logger.Error(_batchLog.Print(_logId, $"Error from BrokerProcessCandle", e));
                return false;
            }
        }

        private static void BuildResponse(CandleMessage request, IEnumerable<Candle> candles)
        {
            var distinctCandle = candles.Distinct();

            var candleDto = distinctCandle.Select(c => new CandleDto()
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