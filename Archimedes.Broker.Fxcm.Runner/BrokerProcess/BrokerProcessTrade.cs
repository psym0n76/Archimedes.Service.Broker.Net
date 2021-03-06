﻿using Archimedes.Library.Message;
using Archimedes.Library.Message.Dto;
using Fx.Broker.Fxcm;
using Fx.Broker.Fxcm.Models;
using NLog;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Archimedes.Library.RabbitMq;

namespace Archimedes.Broker.Fxcm.Runner
{
    public class BrokerProcessTrade : IBrokerProcessTrade
    {
        private static readonly EventWaitHandle SyncResponseEvent = new EventWaitHandle(false, EventResetMode.AutoReset);
        private readonly IProducer<TradeMessage> _producer;
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public BrokerProcessTrade(IProducer<TradeMessage> producer)
        {
            _producer = producer;
        }

        public void Run(TradeMessage request)
        {

            Task.Run(() =>
            {
                _logger.Info("Process Market Order");

                var session = BrokerSession.GetInstance();

                if (session.State == SessionState.Disconnected)
                {
                    session.Connect();
                }

                if (session.State == SessionState.Disconnected)
                {
                    _logger.Error("Unable to connect to FCXM");
                    return;
                }

                session.Subscribe(TradingTable.OpenPosition);
                session.Subscribe(TradingTable.Order);
                //session.OpenPositionUpdate += Session_OpenPositionUpdate;

                session.OpenPositionUpdate += (action, obj) =>
                {
                    if (action != UpdateAction.Insert) return;

                    _logger.Info($"{Enum.GetName(typeof(UpdateAction), action)} Trade ID: {obj.TradeId}; Amount: {obj.AmountK}; Rate: {obj.Open}");

                    SyncResponseEvent.Set();
                    PostTradeIdToQueue(obj.TradeId);
                };


                session.OrderUpdate += Session_OrderUpdate;

                CreateMarketOrder(session, request);

                if (!SyncResponseEvent.WaitOne(30000)) //wait 30 sec
                {
                    throw new Exception("Response waiting timeout expired");
                }

                session.Unsubscribe(TradingTable.OpenPosition);
                session.Unsubscribe(TradingTable.Order);
                session.OrderUpdate -= Session_OrderUpdate;
                //session.OpenPositionUpdate -= Session_OpenPositionUpdate;

            }).ConfigureAwait(false);
        }

        private void Session_OrderUpdate(UpdateAction action, Order obj)
        {
            if (action == UpdateAction.Insert || action == UpdateAction.Delete)
            {
                _logger.Info($"{Enum.GetName(typeof(UpdateAction), action)} OrderID: {obj.OrderId}");
            }
        }

        //private static void Session_OpenPositionUpdate(UpdateAction action, OpenPosition obj)
        //{
        //    if (action != UpdateAction.Insert) return;
        //    Console.WriteLine(
        //        $"{Enum.GetName(typeof(UpdateAction), action)} Trade ID: {obj.TradeId}; Amount: {obj.AmountK}; Rate: {obj.Open}");
        //    SyncResponseEvent.Set();
        //    PostTradeIdToQueue(obj.TradeId);
        //}

        private void CreateMarketOrder(Session session, TradeMessage request)
        {
            _logger.Info("Create Market Order");
            var openTradeParams = new OpenTradeParams();

            if (!string.IsNullOrEmpty(request.Account))
            {
                openTradeParams.AccountId = request.Account;
            }
            else
            {
                var accounts = session.GetAccounts();
                foreach (var account in accounts)
                {
                    if (string.IsNullOrEmpty(account.AccountId)) continue;
                    openTradeParams.AccountId = account.AccountId;
                    break;
                }
            }

            openTradeParams.Amount = request.Lots;
            openTradeParams.Symbol = request.Market;
            openTradeParams.IsBuy = request.BuySell == "B";
            openTradeParams.OrderType = "AtMarket";
            openTradeParams.TimeInForce = "GTC";

            session.OpenTrade(openTradeParams);
        }

        private void PostTradeIdToQueue(string tradeId)
        {
            _logger.Info($"Post Market Order {tradeId}");

            var trade = new TradeMessage()
            {
                Trades = new List<TradeDto>()
                {
                    new TradeDto()
                    {
                        Market = tradeId
                    }
                }
            };

            _producer.PublishMessage(trade,nameof(trade));
        }
    }
}
