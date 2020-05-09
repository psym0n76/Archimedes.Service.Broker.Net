using Fx.Broker.Fxcm.Models;
using System;
using System.Threading;
using System.Threading.Tasks;
//using Fx.MessageBus.Publishers;

namespace Fx.Broker.Fxcm.Runner
{
    public class BrokerProcessTrade : IBrokerProcess
    {
        private static readonly EventWaitHandle SyncResponseEvent = new EventWaitHandle(false, EventResetMode.AutoReset);

        public void Run(Session session, SampleParams sampleParams)
        {

            Task.Run(() =>
            {
                Console.WriteLine("Process Market Order");
                session.Subscribe(TradingTable.OpenPosition);
                session.Subscribe(TradingTable.Order);
                session.OpenPositionUpdate += Session_OpenPositionUpdate;
                session.OrderUpdate += Session_OrderUpdate;

                CreateMarketOrder(session, sampleParams);

                if (!SyncResponseEvent.WaitOne(30000)) //wait 30 sec
                {
                    throw new Exception("Response waiting timeout expired");
                }

                session.Unsubscribe(TradingTable.OpenPosition);
                session.Unsubscribe(TradingTable.Order);
                session.OrderUpdate -= Session_OrderUpdate;
                session.OpenPositionUpdate -= Session_OpenPositionUpdate;

            }).ConfigureAwait(false);
        }

        private static void Session_OrderUpdate(UpdateAction action, Order obj)
        {
            if (action == UpdateAction.Insert || action == UpdateAction.Delete)
            {
                Console.WriteLine($"{Enum.GetName(typeof(UpdateAction), action)} OrderID: {obj.OrderId}");
            }
        }

        private static void Session_OpenPositionUpdate(UpdateAction action, OpenPosition obj)
        {
            if (action != UpdateAction.Insert) return;
            Console.WriteLine(
                $"{Enum.GetName(typeof(UpdateAction), action)} Trade ID: {obj.TradeId}; Amount: {obj.AmountK}; Rate: {obj.Open}");
            SyncResponseEvent.Set();
            PostTradeIdToQueue(obj.TradeId);
        }

        private static void CreateMarketOrder(Session session, SampleParams sampleParams)
        {
            Console.WriteLine("Create Market Order");
            var openTradeParams = new OpenTradeParams();

            if (!string.IsNullOrEmpty(sampleParams.Account))
            {
                openTradeParams.AccountId = sampleParams.Account;
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

            openTradeParams.Amount = sampleParams.Lots ?? 1;
            openTradeParams.Symbol = sampleParams.Instrument;
            openTradeParams.IsBuy = sampleParams.BuySell == "B";
            openTradeParams.OrderType = "AtMarket";
            openTradeParams.TimeInForce = "GTC";

            session.OpenTrade(openTradeParams);
        }

        private static void PostTradeIdToQueue(string tradeId)
        {
            Console.WriteLine($"Post Market Order {tradeId}");

            //INetQPublish p = new NetQPublish();
            //p.PublishMessage(tradeId);
        }
    }
}
