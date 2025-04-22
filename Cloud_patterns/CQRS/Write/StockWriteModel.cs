using System.Collections.Generic;
using System.Threading.Tasks;
using Cloud_patterns.Events;
using Cloud_patterns.PubSub;

namespace Cloud_patterns.CQRS.Write
{
    // Write model (Command side of CQRS)
    public class StockWriteModel
    {
        private readonly Dictionary<string, StockWriteData> _stocks = new();
        private readonly AzureServiceBusEventBroker _eventBroker;

        public StockWriteModel(AzureServiceBusEventBroker eventBroker)
        {
            _eventBroker = eventBroker;
            InitializeStocks();
        }
        
        private void InitializeStocks()
        {
            // Initialize with some stocks
            _stocks["MSFT"] = new StockWriteData { Symbol = "MSFT", Price = 350.0m, Volume = 1_000_000 };
            _stocks["AAPL"] = new StockWriteData { Symbol = "AAPL", Price = 175.0m, Volume = 2_000_000 };
            _stocks["AMZN"] = new StockWriteData { Symbol = "AMZN", Price = 125.0m, Volume = 1_500_000 };
            _stocks["GOOGL"] = new StockWriteData { Symbol = "GOOGL", Price = 140.0m, Volume = 800_000 };
            _stocks["TSLA"] = new StockWriteData { Symbol = "TSLA", Price = 225.0m, Volume = 3_000_000 };
        }

        // Command to update the stock price
        public async Task UpdateStockPrice(string symbol, decimal newPrice)
        {
            if (_stocks.TryGetValue(symbol, out var stock))
            {
                var previousPrice = stock.Price;
                stock.Price = newPrice;
                
                var @event = new StockPriceUpdatedEvent("StockWriteModel", symbol, previousPrice, newPrice);
                
                // Publish event for the read side to update
                await _eventBroker.PublishAsync(@event);
            }
        }

        // Command to update stock trading volume
        public async Task UpdateStockVolume(string symbol, long additionalVolume)
        {
            if (_stocks.TryGetValue(symbol, out var stock))
            {
                stock.Volume += additionalVolume;
                
                var @event = new StockVolumeUpdatedEvent("StockWriteModel", symbol, stock.Volume);
                
                // Publish event for the read side to update
                await _eventBroker.PublishAsync(@event);
            }
        }
    }
}