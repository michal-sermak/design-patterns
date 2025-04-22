using System;
using System.Threading;
using System.Threading.Tasks;
using Cloud_patterns.CQRS.Write;

namespace Cloud_patterns.DataFeed
{
    // Simulates an external stock data feed sending updates
    public class StockDataFeed
    {
        private readonly StockWriteModel _writeModel;
        private readonly Random _random = new Random();
        private readonly string[] _symbols = { "MSFT", "AAPL", "AMZN", "GOOGL", "TSLA" };

        public StockDataFeed(StockWriteModel writeModel)
        {
            _writeModel = writeModel;
        }

        public async Task StartProcessing(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    // Simulate receiving stock data from an external feed
                    var symbol = _symbols[_random.Next(_symbols.Length)];
                    
                    // Simulate either a price update or volume update
                    if (_random.Next(2) == 0)
                    {
                        // Price update - random fluctuation between -2% and +2%
                        var priceChange = (_random.NextDouble() * 4 - 2) / 100;
                        var basePrice = symbol switch
                        {
                            "MSFT" => 350.0m,
                            "AAPL" => 175.0m,
                            "AMZN" => 125.0m,
                            "GOOGL" => 140.0m,
                            "TSLA" => 225.0m,
                            _ => 100.0m
                        };
                        var newPrice = basePrice * (1 + (decimal)priceChange);
                        await _writeModel.UpdateStockPrice(symbol, newPrice);
                    }
                    else
                    {
                        // Volume update - random trading activity
                        var volumeChange = _random.Next(1000, 50000);
                        await _writeModel.UpdateStockVolume(symbol, volumeChange);
                    }
                    
                    await Task.Delay(_random.Next(500, 1500), cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    // Expected when cancellation is requested
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in stock data feed: {ex.Message}");
                    await Task.Delay(1000, cancellationToken); // Wait before retrying
                }
            }
        }
    }
}