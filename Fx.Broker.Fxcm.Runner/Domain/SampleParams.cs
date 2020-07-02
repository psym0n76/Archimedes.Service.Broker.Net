using System;
using System.Collections.Specialized;
using System.Globalization;
using EasyNetQ;

namespace Fx.Broker.Fxcm.Runner
{
    public class SampleParams
    {
        public SampleParams(NameValueCollection args)
        {
            RabbitHutchConnection = GetArgument(args, "RabbitHutchConnection");
            AccessToken = GetRequiredArgument(args, "AccessToken");
            Account = GetArgument(args, "Account");
            Url = GetRequiredArgument(args, "URL");
            Instrument = GetRequiredArgument(args, "Instrument");
            Timeframe = GetArgument(args, "Timeframe") ?? "m1"; ;
            DateFrom = GetDateTime(args, "DateFrom");
            DateTo = GetDateTime(args, "DateTo");
            DateTo = DateTo == DateTime.MinValue ? DateTime.Now : DateTo;
            Count = GetCount(args);
            Lots = GetLots(args);
        }

        public string RabbitHutchConnection { get; }

        public string AccessToken { get; }

        public string Url { get; }

        public string Instrument { get; }

        public string BuySell { get; }

        public int? Lots { get; }

        public string Account { get; }

        public string Timeframe { get; }

        public DateTime DateFrom { get; }

        public DateTime DateTo { get; }

        public int Count { get; }

        public override string ToString()
        {
            return
                $"\n AccessToken={AccessToken}\n Url={Url}\n Account={Account}\n Instrument={Instrument}\n TimeFrame={Timeframe}" +
                $"\n BuySell={BuySell}\n Lots={Lots}\n DateFrom={DateFrom}\n DateTo={DateTo}\n RabbitHutchConnection={RabbitHutchConnection}";
        }

        private static int GetLots(NameValueCollection args)
        {
            var sLots = GetRequiredArgument(args, "Lots");
            return !int.TryParse(sLots, out var lots) ? 1 : lots;
        }

        private static string GetRequiredArgument(NameValueCollection args, string sArgumentName)
        {
            var sArgument = args[sArgumentName];
            if (!string.IsNullOrEmpty(sArgument))
            {
                sArgument = sArgument.Trim();
            }
            if (string.IsNullOrEmpty(sArgument) || sArgument.IndexOfAny(new[] { '{', '}' }) >= 0)
            {
                throw new Exception($"Please provide {sArgumentName} in configuration file");
            }
            return sArgument;
        }

        private static string GetArgument(NameValueCollection args, string sArgumentName)
        {
            var sArgument = args[sArgumentName];
            if (string.IsNullOrEmpty(sArgument) || sArgument.IndexOfAny(new[] { '{', '}' }) >= 0)
            {
                sArgument = "";
            }

            return sArgument;
        }

        private DateTime GetDateTime(NameValueCollection args, string paramName)
        {
            string sDateFormat = "MM.dd.yyyy HH:mm:ss";
            string sDateTime = args[paramName];
            DateTime dateTime;
            if (!DateTime.TryParseExact(sDateTime, sDateFormat, CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeLocal, out dateTime))
            {
                return DateTime.MinValue;
            }
            else
            {
                if (DateTime.Compare(dateTime, DateTime.Now) >= 0)
                {
                    throw new Exception(string.Format("\"{0}\" value {1} is invalid; please fix the value in the configuration file", paramName, sDateTime));
                }
            }

            return dateTime;
        }

        private int GetCount(NameValueCollection args)
        {
            const int exceptValue = -1;
            string sCount = GetArgument(args, "Count") ?? "";

            if (string.IsNullOrEmpty(sCount) == true)
            {
                return exceptValue;
            }

            int count = 0;
            if (!Int32.TryParse(sCount, out count))
                return exceptValue;
            else
                return count;
        }
    }
}