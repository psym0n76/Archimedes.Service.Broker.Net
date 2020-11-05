using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Archimedes.Library.Message.Dto;

namespace Archimedes.Broker.Fxcm.Runner.Http
{
    public interface IMarketClient
    {
        Task<IList<MarketDto>> GetMarketAsync(CancellationToken ct = default);
        //Task UpdateMarketStatusAsync(CancellationToken ct = default);
    }
}