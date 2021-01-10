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

        public BrokerProcessCandle(IProducerFanout<CandleMessage> producerFanout)
        {
            _producerFanout = producerFanout;
        }

        public Task Run(CandleMessage message)
        {
            _logId = _batchLog.Start();
            var reconnect = 1;
            var session = BrokerSession.GetInstance();

            if (session.State == SessionState.Disconnected)
            {
                _batchLog.Update(_logId, $"Connection status: {session.State}");
                session.Connect();
            }

            while (session.State == SessionState.Reconnecting && reconnect < 10)
            {
                _batchLog.Update(_logId,
                    $"Waiting to reconnect for CandleRequest...{reconnect} Market: {message.Market} Timeframe: {message.TimeFrame} Interval: {message.Interval}");
                reconnect++;
                Thread.Sleep(5000);
            }

            switch (session.State)
            {
                case SessionState.Disconnected:

                    _logger.Error(_batchLog.Print(_logId, $"Unable to connect: {session.State}"));
                    return Task.CompletedTask;

                case SessionState.Connected:

                    _batchLog.Update(_logId, $"Connection status: {session.State}");

                    CandleHistory(session, message);
                    PublishCandles(message);

                    _logger.Info(_batchLog.Print(_logId));
                    
                    return Task.CompletedTask;

                case SessionState.Reconnecting:

                    _logger.Error(_batchLog.Print(_logId,
                        $"Candle History: FXCM Reconnection limit hit : {reconnect}"));
                    return Task.CompletedTask;


                default:
                    _logger.Error(_batchLog.Print(_logId, $"Unknown SessionState : {session.State}"));
                    return Task.CompletedTask;

            }
        }

        private void PublishCandles(CandleMessage message)
        {
            _producerFanout.PublishMessage(message, message.QueueName);
            _batchLog.Update(_logId,
                $"Publish to {message.QueueName}: {message.Market} {message.Interval}{message.TimeFrame}");
        }


        private void CandleHistory(Session session, CandleMessage request)
        {
            try
            {
                request.CountCandleIntervals();

                var candles = session.GetCandles(request.ExternalMarketId, request.TimeFrameBroker, request.Intervals,
                    request.StartDate, request.EndDate);

                _batchLog.Update(_logId,
                    $"FXCM Candle Response: Records: {candles.Count}");

                BuildResponse(request, candles);
            }
            catch (Exception e)
            {
                _logger.Error(_batchLog.Print(_logId, $"Error from BrokerProcessCandle", e));
            }
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