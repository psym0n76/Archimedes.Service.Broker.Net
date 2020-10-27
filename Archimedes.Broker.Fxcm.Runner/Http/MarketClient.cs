using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Archimedes.Library.Extensions;
using Archimedes.Library.Message.Dto;
using NLog;

namespace Archimedes.Broker.Fxcm.Runner.Http
{
    public class MarketClient : IMarketClient
    {
        private static readonly HttpClient Client = new HttpClient();
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public async Task<IList<MarketDto>> GetMarketAsync(CancellationToken ct = default)
        {
            var url = ConfigurationManager.AppSettings["ApiRepositoryUrl"].ToString();

            var response = await Client.GetAsync($"{url}/market", ct);

            if (response.IsSuccessStatusCode)
            {
                var markets = await response.Content.ReadAsAsync<IList<MarketDto>>();

                var marketMessage = markets.Aggregate("", (current, market) => current + $"{market}\n");

                _logger.Info($"Successfully received Market {marketMessage} from {url}/market");

                return markets;
            }

            _logger.Warn($"Failed to Get {response.ReasonPhrase}");
            return Array.Empty<MarketDto>();
        }

        public async Task UpdateMarketStatusAsync(CancellationToken ct = default)
        {

            var url = ConfigurationManager.AppSettings["ApiRepositoryUrl"].ToString();

            var marketDto = new MarketDto() {Active = false, Id = 21};

            var payload = new JsonContent(marketDto);

            var response = await Client.PutAsync($"{url}/market_status", payload, ct);

            if (response.IsSuccessStatusCode)
            {
                _logger.Info($"Successfully updated MarketStatus {marketDto.Active}");
                return;
            }

            _logger.Warn($"Failed to Get {response.ReasonPhrase} from {url}/market_status");
        }
    }
}