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

using System.Configuration;
using Archimedes.Library.EasyNetQ;
using Archimedes.Library.Message;

namespace Archimedes.Service.Broker.Net.DependencyResolution
{
    using StructureMap.Configuration.DSL;
    using StructureMap.Graph;

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

            For<INetQPublish<ResponsePrice>>().Use<NetQPublish<ResponsePrice>>()
                .Ctor<string>("host").Is(ConfigurationManager.AppSettings["RabbitHutchConnection"]);



            For<INetQPublish<ResponseCandle>>().Use<NetQPublish<ResponseCandle>>()
                .Ctor<string>("host").Is(ConfigurationManager.AppSettings["RabbitHutchConnection"]);

            For<INetQPublish<ResponseTrade>>().Use<NetQPublish<ResponseTrade>>()
                .Ctor<string>("host").Is(ConfigurationManager.AppSettings["RabbitHutchConnection"]);

            For<IQueueTesting>().Use<QueueTesting>();
            For<IPriceSubscriber>().Use<PriceSubscriber>();
            For<ICandleSubscriber>().Use<CandleSubscriber>();
            For<IBrokerProcessCandle>().Use<BrokerProcessCandle>();
            For<IBrokerProcessPrice>().Use<BrokerProcessPriceTest>();

            For<IMessageBrokerConsumer>().Use<MessageBrokerConsumer>();
        }

        #endregion
    }
}