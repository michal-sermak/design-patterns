using System;
using System.Collections.Generic;
using System.Linq;
using Cloud_patterns.Events;
using Cloud_patterns.PubSub;

namespace Cloud_patterns.Subscribers
{
    // Analytics engine
    public class AnalyticsEngine : SubscriberBase
    {
        private readonly Dictionary<string, List<decimal>> _priceHistory = new();

        public AnalyticsEngine(string name, AzureServiceBusEventBroker eventBroker) : base(name, eventBroker)
        {
        }

        protected override void OnStockPriceUpdated(StockPriceUpdatedEvent @event)
        {
            if (!_priceHistory.TryGetValue(@event.Symbol, out var history))
            {
                history = new List<decimal>();
                _priceHistory[@event.Symbol] = history;
            }

            history.Add(@event.NewPrice);
            
            // Only log analytics occasionally to reduce console spam
            if (history.Count % 10 == 0)
            {
                var average = history.TakeLast(10).Average();
                Console.WriteLine($"[{_name}] 10-point moving average for {@event.Symbol}: ${average:F2}");
            }
        }
    }
}