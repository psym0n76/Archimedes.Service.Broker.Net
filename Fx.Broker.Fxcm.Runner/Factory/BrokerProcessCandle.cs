using Fx.Broker.Fxcm.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Archimedes.Library.Message;
using Archimedes.Library.Message.Dto;
//using Fx.MessageBus.Publishers;


// ReSharper disable once CheckNamespace
namespace Fx.Broker.Fxcm.Runner
{
    public class BrokerProcessCandle : IBrokerProcess
    {
        public void Run(Session session, SampleParams sampleParams)
        {
            Task.Run(() =>
            {
                Console.WriteLine($"Process Candle History: {sampleParams}");

                var candleHistory = GetHistory(session, sampleParams);

                var candleHistoryDto = GetCandleHistoryDto(candleHistory);

                PublishMessage(candleHistoryDto);
                PrintHistory(candleHistory);

            }).ConfigureAwait(false);
        }

        private static ResponseCandle GetCandleHistoryDto(IList<Candle> candle)
        {
            if (candle == null)
            {
                Console.WriteLine("Candle empty");
                return null;
            }

            var result = new ResponseCandle { Text = "something", Payload = new List<CandleDto>() };

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

        private static void PublishMessage(ResponseCandle message)
        {
            //INetQPublish p = new NetQPublish();
            //p.PublishCandleMessage(message);
        }

        private static IList<Candle> GetHistory(Session session, SampleParams sampleParams)
        {
            var offers = session.GetOffers();
            var offer = offers.FirstOrDefault(o => o.Currency == sampleParams.Instrument);
            if (offer == null)
            {
                throw new Exception($"The instrument '{sampleParams.Instrument}' is not valid");
            }

            if (sampleParams.DateFrom != DateTime.MinValue && sampleParams.DateTo == DateTime.MinValue)
            {
                throw new Exception("Please provide DateTo in configuration file");
            }

            return session.GetCandles(offer.OfferId, sampleParams.Timeframe, sampleParams.Count,
                sampleParams.DateFrom, sampleParams.DateTo);
        }

        private static void PrintHistory(IEnumerable<Candle> candleHistory)
        {
            foreach (var candle in candleHistory)
            {
                Console.WriteLine($"DateTime={candle.Timestamp}, BidOpen={candle.BidOpen}, BidClose={candle.BidClose}, AskOpen={candle.AskOpen}, AskClose={candle.AskClose}");
            }
        }
    }
}