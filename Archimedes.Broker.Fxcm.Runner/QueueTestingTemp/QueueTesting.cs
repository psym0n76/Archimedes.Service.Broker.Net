using Archimedes.Library.Message;
using Archimedes.Library.Message.Dto;
using NLog;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Archimedes.Library.EasyNetQ;

namespace Archimedes.Broker.Fxcm.Runner
{
    public class QueueTesting : IQueueTesting
    {
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly INetQPublish<ResponsePrice> _netQPublish;

        public QueueTesting(INetQPublish<ResponsePrice> netQPublish)
        {
            _netQPublish = netQPublish;
        }

        public void QueueTest()
        {
            var counter = 0;
            try
            {
                Task.Run(() =>
                {
                    _logger.Info("Running Test Queue");

                    while (true)
                    {
                        var price = new ResponsePrice()
                        {
                            Status = "Test",
                            Payload = new List<PriceDto>()
                            {
                                new PriceDto()
                                {
                                    Market = "GBPUSD",
                                    Timestamp = DateTime.Now,
                                    BidOpen = 1.34, BidHigh = 1.40, BidLow = 1.3, BidClose = 1.39, AskOpen = 1.34,
                                    AskHigh = 1.40, AskLow = 1.3, AskClose = 1.39, Granularity = "15",TickQty = 25
                                },
                                new PriceDto()
                                {
                                    Market = "GBPUSD",
                                    Timestamp = DateTime.Now,
                                    BidOpen = 1.34, BidHigh = 1.40, BidLow = 1.3, BidClose = 1.39, AskOpen = 1.34,
                                    AskHigh = 1.40, AskLow = 1.3, AskClose = 1.39, Granularity = "15",TickQty = 25
                                }
                            },
                            Text = "Test Message"
                        };

                        _logger.Info($"MTest Message No. {counter++} Message \n {price}");
                        _netQPublish.PublishMessage(price);
                        Thread.Sleep(30000);

                    }
                });
            }
            catch (Exception e)
            {
                _logger.Error($"Error found: Message:{e.Message} StackTrace:{e.StackTrace}");
            }
        }
    }
}