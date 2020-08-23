using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Archimedes.Library.Domain;
using Archimedes.Library.Extensions;
using Archimedes.Library.Message.Dto;
using Microsoft.Extensions.Options;
using NLog;

namespace Archimedes.Broker.Fxcm.Runner.Http
{
    public class MarketClient : IMarketClient
    {
        private readonly HttpClient _client;
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public MarketClient(HttpClient httpClient, IOptions<Config> config)
        {
            httpClient.BaseAddress = new Uri($"{config.Value.ApiRepositoryUrl}");
            httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
            _client = httpClient;
        }

        public async Task<IList<MarketDto>> GetMarketAsync(CancellationToken ct = default)
        {
            var response = await _client.GetAsync($"market", ct);

            if (response.IsSuccessStatusCode)
            {
                var markets = await response.Content.ReadAsAsync<IList<MarketDto>>();

                var marketMessage = markets.Aggregate("", (current, market) => current + $"{market}\n");

                _logger.Info($"Successfully received Market {marketMessage}");

                return markets;
            }

            _logger.Warn($"Failed to Get {response.ReasonPhrase} from {_client.BaseAddress}/market");
            return Array.Empty<MarketDto>();
        }
    }
}