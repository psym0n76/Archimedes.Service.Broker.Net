// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DefaultRegistry.cs" company="Web Advanced">
// Copyright 2012 Web Advanced (www.webadvanced.com)
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0

// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using Archimedes.Broker.Fxcm.Runner;
using Archimedes.Broker.Fxcm.Runner.Http;
using Archimedes.Library.Domain;
using Archimedes.Library.Message;
using Archimedes.Library.RabbitMq;
using Microsoft.Extensions.Options;
using StructureMap;
using System.Configuration;

namespace Archimedes.Service.Broker.Net.DependencyResolution
{
    public class DefaultRegistry : Registry
    {
        #region Constructors and Destructors

        public DefaultRegistry()
        {

            Scan(
                scan =>
                {
                    scan.TheCallingAssembly();
                    scan.WithDefaultConventions();
                    scan.With(new ControllerConvention());
                });

            For<IProducerFanout<CandleMessage>>().Use<ProducerFanout<CandleMessage>>()
                .Ctor<string>("host").Is(ConfigurationManager.AppSettings["RabbitHost"])
                .Ctor<string>("port").Is(ConfigurationManager.AppSettings["RabbitPort"]);


            For<IProducerFanout<PriceMessage>>().Use<ProducerFanout<PriceMessage>>()
                .Ctor<string>("host").Is(ConfigurationManager.AppSettings["RabbitHost"])
                .Ctor<string>("port").Is(ConfigurationManager.AppSettings["RabbitPort"]);


            //For<IProducer<PriceMessage>>().Use<Producer<PriceMessage>>()
            //    .Ctor<string>("host").Is(ConfigurationManager.AppSettings["RabbitHost"])
            //    .Ctor<string>("port").Is(ConfigurationManager.AppSettings["RabbitPort"])
            //    .Ctor<string>("exchange").Is(ConfigurationManager.AppSettings["RabbitExchange"]);

            For<IProducerFanout<PriceMessage>>().Use<ProducerFanout<PriceMessage>>()
                .Ctor<string>("host").Is(ConfigurationManager.AppSettings["RabbitHost"])
                .Ctor<string>("port").Is(ConfigurationManager.AppSettings["RabbitPort"]);

            For<IProducer<TradeMessage>>().Use<Producer<TradeMessage>>()
                .Ctor<string>("host").Is(ConfigurationManager.AppSettings["RabbitHost"])
                .Ctor<string>("port").Is(ConfigurationManager.AppSettings["RabbitPort"])
                .Ctor<string>("exchange").Is(ConfigurationManager.AppSettings["RabbitExchange"]);



            For<ICandleConsumer>().Use<CandleConsumer>()
                .Ctor<string>("host").Is(ConfigurationManager.AppSettings["RabbitHost"])
                .Ctor<string>("port").Is(ConfigurationManager.AppSettings["RabbitPort"])
                .Ctor<string>("exchange").Is(ConfigurationManager.AppSettings["RabbitExchange"])
                .Ctor<string>("queueName").Is("CandleRequestQueue");


            For<IPriceConsumer>().Use<PriceConsumer>()
                .Ctor<string>("host").Is(ConfigurationManager.AppSettings["RabbitHost"])
                .Ctor<string>("port").Is(ConfigurationManager.AppSettings["RabbitPort"])
                .Ctor<string>("exchange").Is(ConfigurationManager.AppSettings["RabbitExchange"])
                .Ctor<string>("queueName").Is("PriceRequestQueue");

            For<IPriceSubscriber>().Use<PriceSubscriber>();
            For<ICandleSubscriber>().Use<CandleSubscriber>();

            For<IBrokerProcessCandle>().Use<BrokerProcessCandle>();
            For<IBrokerProcessTrade>().Use<BrokerProcessTrade>();
            For<IBrokerProcessPrice>().Use<BrokerProcessPrice>();

            //For<IMarketClient>().Use<MarketClient>()
            //    .Ctor<IOptions<Config>>();

            For<IMarketClient>().Use<MarketClient>();
            

            For<IMessageBrokerConsumer>().Use<MessageBrokerConsumer>();
        }

        #endregion
    }
}