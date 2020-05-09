namespace Fx.Broker.Fxcm.Runner
{
    public class BrokerProcessFactory
    {
        public static IBrokerProcess Get(string message)
        {
            switch (message)
            {
                case "Price":
                    //return new BrokerProcessPrice();
                case "Trade":
                    return new BrokerProcessTrade();
                case "Candle":
                    return new BrokerProcessCandle();
                default:
                    return null;
            }
        }
    }
}