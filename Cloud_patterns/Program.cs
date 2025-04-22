using System;
using System.Threading;
using System.Threading.Tasks;
using Cloud_patterns.Configuration;
using Cloud_patterns.CQRS.Read;
using Cloud_patterns.CQRS.Write;
using Cloud_patterns.DataFeed;
using Cloud_patterns.Events;
using Cloud_patterns.PubSub;
using Cloud_patterns.Subscribers;

namespace Cloud_patterns
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Real-Time Stock Trading System Demo (Azure Cloud Patterns)");
            Console.WriteLine("=================================================");
            Console.WriteLine("Demonstrating Publisher-Subscriber using Azure Service Bus and CQRS Patterns");
            Console.WriteLine();

            // Force Azure Service Bus usage
            await RunWithAzureServiceBus();
        }

        static async Task RunWithAzureServiceBus()
        {
            Console.WriteLine("Using Azure Service Bus as event broker");
            
            try
            {
                // Create the Azure Service Bus event broker
                var connectionString = AzureConfig.ServiceBusConnectionString;
                Console.WriteLine($"Connecting to Azure Service Bus...");
                
                using var eventBroker = new AzureServiceBusEventBroker(connectionString);
                
                // Ensure all topics and subscriptions exist
                await eventBroker.EnsureInfrastructureExists();
                
                // Create the read and write models for CQRS
                var stockReadModel = new StockReadModel();
                var stockWriteModel = new StockWriteModel(eventBroker);
                
                // Register the read model to receive stock updates from the event broker
                Console.WriteLine("Setting up event subscriptions...");
                await eventBroker.SubscribeAsync<StockPriceUpdatedEvent>("read-model", 
                    async (e) => { stockReadModel.HandleStockPriceUpdate(e); await Task.CompletedTask; });
                
                await eventBroker.SubscribeAsync<StockVolumeUpdatedEvent>("read-model", 
                    async (e) => { stockReadModel.HandleStockVolumeUpdate(e); await Task.CompletedTask; });
                
                // Create client subscriptions
                await SetupClientSubscriptions(eventBroker);
                
                // Simulate incoming stock data from external feed
                var stockDataFeed = new StockDataFeed(stockWriteModel);
                
                Console.WriteLine("Starting data processing...");
                
                // Start processing stock data
                var cancellationTokenSource = new CancellationTokenSource();
                var dataFeedTask = stockDataFeed.StartProcessing(cancellationTokenSource.Token);
                
                // Simulate clients querying the read model
                var clientQueryTask = SimulateClientQueries(stockReadModel, cancellationTokenSource.Token);
                
                Console.WriteLine("Press any key to stop the simulation...");
                Console.ReadKey();
                
                // Stop all tasks
                cancellationTokenSource.Cancel();
                
                try
                {
                    await Task.WhenAll(dataFeedTask, clientQueryTask);
                }
                catch (OperationCanceledException)
                {
                    Console.WriteLine("Simulation stopped.");
                }
                
                // Display statistics
                Console.WriteLine("\nStatistics:");
                Console.WriteLine($"Total events published: {eventBroker.TotalEventsPublished}");
                Console.WriteLine($"Total events delivered: {eventBroker.TotalEventsDelivered}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error connecting to Azure Service Bus: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }
        
        static async Task SetupClientSubscriptions(AzureServiceBusEventBroker eventBroker)
        {
            
            // Setup dashboard subscription
            await eventBroker.SubscribeAsync<StockPriceUpdatedEvent>("dashboard", async (e) => 
            {
                var changePercent = (e.NewPrice / e.PreviousPrice - 1) * 100;
                Console.WriteLine($"[Dashboard] Updated for {e.Symbol}: ${e.NewPrice:F2} ({changePercent:F2}%)");
                await Task.CompletedTask;
            });
            
            // Setup mobile app subscription
            await eventBroker.SubscribeAsync<StockPriceUpdatedEvent>("mobile", async (e) => 
            {
                if (Math.Abs(e.NewPrice - e.PreviousPrice) / e.PreviousPrice > 0.01m)
                {
                    Console.WriteLine($"[Mobile] Alert: Significant movement for {e.Symbol}!");
                }
                await Task.CompletedTask;
            });
            
            // Setup analytics engine subscription
            await eventBroker.SubscribeAsync<StockPriceUpdatedEvent>("analytics", async (e) => 
            {
                Console.WriteLine($"[Analytics] Processing price data for {e.Symbol}");
                await Task.CompletedTask;
            });
            
            // All clients subscribe to volume updates
            await eventBroker.SubscribeAsync<StockVolumeUpdatedEvent>("all-clients", async (e) => 
            {
                Console.WriteLine($"[All Clients] Volume update for {e.Symbol}: {e.Volume}");
                await Task.CompletedTask;
            });
        }

        static async Task SimulateClientQueries(StockReadModel readModel, CancellationToken cancellationToken)
        {
            var stockSymbols = new[] { "MSFT", "AAPL", "AMZN", "GOOGL", "TSLA" };
            var random = new Random();

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    // Simulate multiple clients querying the read model
                    var symbol = stockSymbols[random.Next(stockSymbols.Length)];
                    var stock = readModel.GetStock(symbol);
                    
                    if (stock != null)
                    {
                        Console.WriteLine($"[QUERY] Client retrieved {symbol}: ${stock.CurrentPrice:F2} (Vol: {stock.Volume})");
                    }
                    
                    await Task.Delay(1000, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in client query simulation: {ex.Message}");
                    await Task.Delay(500, cancellationToken);
                }
            }
        }
    }
}