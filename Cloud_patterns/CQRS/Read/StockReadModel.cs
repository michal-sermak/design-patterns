using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Cloud_patterns.Events;

namespace Cloud_patterns.CQRS.Read
{
    // Read model (Query side of CQRS)
    public class StockReadModel
    {
        private readonly ConcurrentDictionary<string, StockReadData> _stocks = new();

        // Event handler for stock price updates
        public void HandleStockPriceUpdate(StockPriceUpdatedEvent @event)
        {
            _stocks.AddOrUpdate(
                @event.Symbol,
                new StockReadData { Symbol = @event.Symbol, CurrentPrice = @event.NewPrice },
                (_, existingStock) =>
                {
                    existingStock.CurrentPrice = @event.NewPrice;
                    existingStock.PriceChange = @event.NewPrice - @event.PreviousPrice;
                    existingStock.PriceChangePercent = (@event.NewPrice / @event.PreviousPrice - 1) * 100;
                    existingStock.LastUpdated = DateTime.UtcNow;
                    return existingStock;
                }
            );
        }

        // Event handler for stock volume updates
        public void HandleStockVolumeUpdate(StockVolumeUpdatedEvent @event)
        {
            _stocks.AddOrUpdate(
                @event.Symbol,
                new StockReadData { Symbol = @event.Symbol, Volume = @event.Volume },
                (_, existingStock) =>
                {
                    existingStock.Volume = @event.Volume;
                    existingStock.LastUpdated = DateTime.UtcNow;
                    return existingStock;
                }
            );
        }

        // Query methods - optimized for reads
        public StockReadData? GetStock(string symbol)
        {
            _stocks.TryGetValue(symbol, out var stock);
            return stock;
        }

        public List<StockReadData> GetAllStocks()
        {
            return _stocks.Values.ToList();
        }

        public List<StockReadData> GetTopMovers(int count = 5)
        {
            return _stocks.Values
                .OrderByDescending(s => Math.Abs(s.PriceChangePercent))
                .Take(count)
                .ToList();
        }
    }
}