using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using Cloud_patterns.Events;

namespace Cloud_patterns.PubSub
{
    /// <summary>
    /// Azure Service Bus implementation of the event broker.
    /// </summary>
    public class AzureServiceBusEventBroker : IDisposable
    {
        private readonly ServiceBusClient _client;
        private readonly ServiceBusAdministrationClient _adminClient;
        private readonly Dictionary<Type, ServiceBusSender> _senders = new();
        private readonly List<ServiceBusProcessor> _processors = new();
        private readonly string _connectionString;
        
        // Statistics
        public int TotalEventsPublished { get; private set; } = 0;
        public int TotalEventsDelivered { get; private set; } = 0;

        public AzureServiceBusEventBroker(string connectionString)
        {
            _connectionString = connectionString;
            _client = new ServiceBusClient(connectionString);
            _adminClient = new ServiceBusAdministrationClient(connectionString);
            Console.WriteLine($"Azure Service Bus broker initialized with endpoint: {connectionString.Split(';')[0]}");
        }

        /// <summary>
        /// Ensures that all required topics and subscriptions exist
        /// </summary>
        public async Task EnsureInfrastructureExists()
        {
            // Create topics if they don't exist
            string[] topicNames = { "stock-price-updates", "stock-volume-updates" };
            
            foreach (var topicName in topicNames)
            {
                try
                {
                    if (!await _adminClient.TopicExistsAsync(topicName))
                    {
                        Console.WriteLine($"Creating topic: {topicName}");
                        await _adminClient.CreateTopicAsync(topicName);
                        Console.WriteLine($"Topic {topicName} created successfully");
                    }
                    else
                    {
                        Console.WriteLine($"Topic {topicName} already exists");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error with topic {topicName}: {ex.Message}");
                    throw;
                }
            }
            
            // Create subscriptions if they don't exist
            var subscriptions = new Dictionary<string, string[]>
            {
                ["stock-price-updates"] = new[] { "dashboard-subscription", "mobile-subscription", "analytics-subscription", "read-model-subscription" },
                ["stock-volume-updates"] = new[] { "all-clients-subscription", "read-model-subscription" }
            };
            
            foreach (var topicEntry in subscriptions)
            {
                string topicName = topicEntry.Key;
                foreach (var subscriptionName in topicEntry.Value)
                {
                    try
                    {
                        if (!await _adminClient.SubscriptionExistsAsync(topicName, subscriptionName))
                        {
                            Console.WriteLine($"Creating subscription: {subscriptionName} for topic: {topicName}");
                            await _adminClient.CreateSubscriptionAsync(topicName, subscriptionName);
                            Console.WriteLine($"Subscription {subscriptionName} created successfully");
                        }
                        else
                        {
                            Console.WriteLine($"Subscription {subscriptionName} already exists");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error with subscription {subscriptionName}: {ex.Message}");
                        throw;
                    }
                }
            }
            
            Console.WriteLine("Service Bus infrastructure is ready.");
        }

        /// <summary>
        /// Maps event types to Azure Service Bus topics
        /// </summary>
        private string GetTopicName<TEvent>() where TEvent : IEvent
        {
            if (typeof(TEvent) == typeof(StockPriceUpdatedEvent))
            {
                return "stock-price-updates";
            }
            else if (typeof(TEvent) == typeof(StockVolumeUpdatedEvent))
            {
                return "stock-volume-updates";
            }
            
            // Default fallback
            return typeof(TEvent).Name.ToLowerInvariant();
        }

        /// <summary>
        /// Subscribe to events of a specific type with the given client name
        /// </summary>
        public async Task SubscribeAsync<TEvent>(string clientName, Func<TEvent, Task> handler) 
            where TEvent : IEvent
        {
            string topicName = GetTopicName<TEvent>();
            string subscriptionName = $"{clientName}-subscription";
            
            Console.WriteLine($"[AZURE-SB] Subscribing to Azure Service Bus topic '{topicName}' with subscription '{subscriptionName}'");
            
            // Create a processor for this topic and subscription
            var processorOptions = new ServiceBusProcessorOptions
            {
                MaxConcurrentCalls = 1,
                AutoCompleteMessages = true
            };
            
            var processor = _client.CreateProcessor(topicName, subscriptionName, processorOptions);
            
            // Add the message handler
            processor.ProcessMessageAsync += async (args) =>
            {
                try
                {
                    string json = args.Message.Body.ToString();
                    Console.WriteLine($"[AZURE-SB] Received message: {json}");
                    
                    TEvent? eventObj = JsonSerializer.Deserialize<TEvent>(json);
                    
                    if (eventObj != null)
                    {
                        Console.WriteLine($"[AZURE-SB] Processing message: {args.Message.MessageId} on topic {topicName}");
                        await handler(eventObj);
                        TotalEventsDelivered++;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing message: {ex.Message}");
                }
            };
            
            // Handle any errors while processing messages
            processor.ProcessErrorAsync += (args) =>
            {
                Console.WriteLine($"Error in Service Bus processor: {args.Exception.Message}");
                return Task.CompletedTask;
            };
            
            // Start processing
            await processor.StartProcessingAsync();
            
            // Keep track of processors to dispose them later
            _processors.Add(processor);
            
            Console.WriteLine($"Subscribed to {topicName} with client {clientName}");
        }

        /// <summary>
        /// Publish an event to the appropriate topic
        /// </summary>
        public async Task PublishAsync<TEvent>(TEvent @event) where TEvent : IEvent
        {
            string topicName = GetTopicName<TEvent>();
            
            Console.WriteLine($"[AZURE-SB] Publishing to Azure Service Bus topic '{topicName}'");
            
            try
            {
                // Get or create a sender for this topic
                if (!_senders.TryGetValue(typeof(TEvent), out var sender))
                {
                    sender = _client.CreateSender(topicName);
                    _senders[typeof(TEvent)] = sender;
                }
                
                // Serialize the event to JSON
                string json = JsonSerializer.Serialize(@event);
                
                // Create a message with additional properties
                var message = new ServiceBusMessage(json)
                {
                    MessageId = @event.Id.ToString(),
                    Subject = @event.Source,
                    ContentType = "application/json",
                };
                
                // Add custom properties for filtering
                if (@event is StockPriceUpdatedEvent priceEvent)
                {
                    message.ApplicationProperties.Add("Symbol", priceEvent.Symbol);
                    message.ApplicationProperties.Add("PriceChange", 
                        (priceEvent.NewPrice - priceEvent.PreviousPrice) / priceEvent.PreviousPrice);
                }
                else if (@event is StockVolumeUpdatedEvent volumeEvent)
                {
                    message.ApplicationProperties.Add("Symbol", volumeEvent.Symbol);
                    message.ApplicationProperties.Add("Volume", volumeEvent.Volume);
                }
                
                // Send the message
                Console.WriteLine($"[AZURE-SB] Sending message: {json}");
                await sender.SendMessageAsync(message);
                TotalEventsPublished++;
                
                Console.WriteLine($"[AZURE-SB] Published {typeof(TEvent).Name} to topic {topicName}, ID: {message.MessageId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AZURE-SB] Error publishing message: {ex.Message}");
                Console.WriteLine($"[AZURE-SB] Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        /// <summary>
        /// Dispose of all resources
        /// </summary>
        public async void Dispose()
        {
            foreach (var processor in _processors)
            {
                try
                {
                    await processor.StopProcessingAsync();
                    await processor.DisposeAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error disposing processor: {ex.Message}");
                }
            }
            
            foreach (var sender in _senders.Values)
            {
                try
                {
                    await sender.DisposeAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error disposing sender: {ex.Message}");
                }
            }
            
            await _client.DisposeAsync();
        }
    }
}