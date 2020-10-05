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

        public void Run(CandleMessage message)
        {
            //todo potentially move to sequential
            Task.Run(() =>
            {
                var reconnect = 0;
                var session = BrokerSession.GetInstance();

                _logger.Info($"Current connection status: {session.State}");

                if (session.State == SessionState.Disconnected)
                {
                    session.Connect();
                }


                while (session.State == SessionState.Reconnecting && reconnect < 10)
                {
                    _logger.Info($"Waiting to reconnect...{reconnect++}");
                    Thread.Sleep(5000);
                }


                if (session.State == SessionState.Disconnected)
                {
                    _logger.Error($"Unable to connect to FXCM: {session.State}");
                    return;
                }

                if (session.State == SessionState.Connected)
                {
                    _logger.Info($"Connected to FXCM {session.State}");
                    _logger.Info($"Process Candle History: {message}");

                    try
                    {
                        CandleHistory(session, message);
                    }

                    catch (InvalidOperationException e)
                    {
                        _logger.Error($"Candle History: FXCM Connection Failed: {e.Message}");
                    }
                    catch (Exception e)
                    {
                        _logger.Error($"Candle History: Unknown error returned from FXCM: {e.Message} {e.StackTrace} {e.InnerException}");
                        return;
                    }


                    if (message.Success)
                    {

                        _producer.PublishMessage(message, "CandleResponseQueue");
                        _logger.Info($"Published to CandleResponseQueue: {message}");
                        return;
                    }
                }

                if (reconnect==10)
                {
                    _logger.Info($"Failed to BrokerProcessCandle TIMEOUT {reconnect} {session.State}");
                }
            });
        }


        private void CandleHistory(Session session, CandleMessage request)
        {
            request.Logs.Add("Candle Response from Broker");

            var offers = session.GetOffers();

            if (offers == null)
            {
                _logger.Warn($"Null returned from Offers: {request}");
                request.Logs.Add($"Null returned from Offers: {request}");
                return;
            }

            _logger.Info($"FXCM Offers returned {offers.Count}");

            // returns no offers
            var offer = offers.FirstOrDefault(o => o.Currency == request.Market);

            if (!ValidateRequest(request, offer))
                return;

            _logger.Info($" Broker Request parameters: " +
                         $"\n  {nameof(offer.OfferId)}: {offer.OfferId} {nameof(request.TimeFrame)}: {request.TimeFrame}" +
                         $"\n  {nameof(request.StartDate)}: {request.StartDate.BrokerDate()} {nameof(request.EndDate)}: {request.EndDate.BrokerDate()}");
            request.CountCandleIntervals();

            var candles = session.GetCandles(offer.OfferId, request.TimeFrameBroker, request.Intervals,
                request.StartDate, request.EndDate);

            _logger.Info($"Records returned from FXCM: {candles.Count}");

            //foreach (var candle in candles)
            //{
            //    _logger.Info(
            //        $" {nameof(candle.Timestamp)}:{candle.Timestamp} {nameof(candle.AskOpen)}:{candle.AskOpen} {nameof(candle.AskHigh)}:{candle.AskHigh} {nameof(candle.AskLow)}:{candle.AskLow} {nameof(candle.AskClose)}:{candle.AskClose}");
            //}

            BuildResponse(request, candles);
        }

        private void BuildResponse(CandleMessage request, IList<Candle> candles)
        {
            _logger.Info($"Build response for {candles.Count} candles");

            var candleDto = candles.Select(c => new CandleDto()
                {
                    TimeStamp = c.Timestamp,
                    ToDate = c.Timestamp,
                    FromDate = c.Timestamp.AddMinutes(-request.Interval),
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
                    Granularity = $"{request.Interval}{request.TimeFrame}"
                })
                .ToList();

            request.Candles = candleDto;
            request.Success = true;
        }

        private bool ValidateRequest(CandleMessage request, Offer offer)
        {
            if (offer == null)
            {
                var message = $"The instrument {request.Market} is not valid: {request}";
                _logger.Info(message);
                //response.Status = message;
                return false;
            }

            if (request.StartDate.BrokerDate() != DateTime.MinValue &&
                request.EndDate.BrokerDate() == DateTime.MinValue)
            {
                var message = $"Incorrect Date formats {request.StartDate.BrokerDate()} {request.EndDate.BrokerDate()}";
                _logger.Info(message);
                //response.Status = message;
                return false;
            }

            return true;
        }
    }
}