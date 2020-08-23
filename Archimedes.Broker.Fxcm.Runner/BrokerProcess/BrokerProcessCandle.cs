﻿using Archimedes.Library.Extensions;
using Archimedes.Library.Message;
using Archimedes.Library.Message.Dto;
using Fx.Broker.Fxcm;
using Fx.Broker.Fxcm.Models;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
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
            Task.Run(() =>
            {
                var session = BrokerSession.GetInstance();

                _logger.Info($"Current connection status: {session.State}");

                if (session.State == SessionState.Disconnected)
                {
                    session.Connect();
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
                    catch (Exception e)
                    {
                        _logger.Error($"Unknown error: Candle History: {e.Message} {e.StackTrace} {e.InnerException}");
                    }


                    if (message.Success)
                    {
                        
                        _producer.PublishMessage(message, "CandleResponseQueue");
                    }
                }

            }).ConfigureAwait(false);
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

            foreach (var offer1 in offers)
            {
                _logger.Info(
                    $"Offers Returned: {nameof(offer1.OfferId)}:{offer1.OfferId}  {nameof(offer1.Currency)}:{offer1.Currency} Rest:{offer1}");
            }


            // returns no offers
            var offer = offers.FirstOrDefault(o => o.Currency == request.Market);

            if (!ValidateRequest(request, offer))
                return;

            _logger.Info($" Broker Request parameters: " +
                         $"\n  {nameof(offer.OfferId)}: {offer.OfferId} {nameof(request.TimeFrame)}: {request.TimeFrame}" +
                         $"\n  {nameof(request.StartDate)}: {request.StartDate.BrokerDate()} {nameof(request.EndDate)}: {request.EndDate.BrokerDate()}" +
                         $"\n  {nameof(request.StartDate)}: {request.StartDate} {nameof(request.EndDate)}: {request.EndDate}");

            var candles = session.GetCandles(offer.OfferId, request.TimeFrame, 1,
                request.StartDate, request.EndDate);

            _logger.Info($"Records returned from FXCM: {candles.Count}");

            foreach (var candle in candles)
            {
                _logger.Info(
                    $" {nameof(candle.Timestamp)}:{candle.Timestamp} {nameof(candle.AskOpen)}:{candle.AskOpen} {nameof(candle.AskHigh)}:{candle.AskHigh} {nameof(candle.AskLow)}:{candle.AskLow} {nameof(candle.AskClose)}:{candle.AskClose}");
            }

            BuildResponse(request, candles);
        }

        private void BuildResponse(CandleMessage request, IList<Candle> candles)
        {

            _logger.Info($"Build response {candles.Count}");

            if (candles == null)
            {
                var message = $"Candle response from Broker empty {request}";
                _logger.Error(message);
                request.Logs.Add(message);
                return;
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