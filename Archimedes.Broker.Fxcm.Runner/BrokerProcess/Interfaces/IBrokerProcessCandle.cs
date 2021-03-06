﻿using System.Threading.Tasks;
using Archimedes.Library.Message;

namespace Archimedes.Broker.Fxcm.Runner
{
    public interface IBrokerProcessCandle
    {
        //void PriceProcessor(CandleMessage message);
        Task CandleProcessor(CandleMessage message);
    }
}