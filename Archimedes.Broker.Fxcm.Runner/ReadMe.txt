GetHistPrices application

Brief
==================================================================================
This sample shows how to load instruments' historical prices.
The sample performs the following actions:
1. Login.
2. Request historical prices for the specified timeframe of the specified period or last trading day. 
3. Print gathered information.
4. Logout.

Building the application
==================================================================================
In order to build this application you will need MS Visual Studio 2015 and
.NET framework 4.5 or later.
You can download .NET framework from http://msdn.microsoft.com/en-us/netframework/

Running the application
==================================================================================
Change the App.config file by putting your information in the "appSettings" section.

Arguments
==================================================================================
{ACCESSTOKEN} - Your Access token. Mandatory argument.
{URL} - The RestAPI URL. Mandatory argument. For example, https://api-demo.fxcm.com
{INSTRUMENT} - An instrument, for which you want to create an order.
        For example, EUR/USD. Mandatory argument.
{TIMEFRAME} - time period which forms a single candle. Mandatory argument.
        Valid values: m1,m5,m15,m30,H1,H2,H3,H4,H6,H8,D1,W1,M1
        For example, m1 - for 1 minute, H1 - for 1 hour.
{DATEFROM} - datetime from which you want to receive historical prices.
        Optional argument. Format is MM.dd.yyyy HH:mm:ss.
{DATETO} - datetime until which you want to receive historical prices.
        Optional argument. If you leave this argument as it is, it will mean to now.
        Format is MM.dd.yyyy HH:mm:ss
{COUNT} - Number of candles which you want to receive historical prices from {DATETO}.
        If time range {DATEFROM} is specified, number of candles parameter is ignored.
        Max Value is 10000.