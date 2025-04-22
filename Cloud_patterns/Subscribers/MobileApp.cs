using System;
using Cloud_patterns.Events;
using Cloud_patterns.PubSub;

namespace Cloud_patterns.Subscribers
{
    // Mobile app subscriber
    public class MobileApp : SubscriberBase
    {
        public MobileApp(string name, AzureServiceBusEventBroker eventBroker) : base(name, eventBroker)
        {
        }

        protected override void OnStockPriceUpdated(StockPriceUpdatedEvent @event)
        {
            var percentChange = (@event.NewPrice - @event.PreviousPrice) / @event.PreviousPrice * 100;
            
            // Only notify on significant price movements
            if (Math.Abs(percentChange) >= 1.0m)
            {
                string direction = percentChange > 0 ? "▲" : "▼";
                Console.WriteLine($"[{_name}] ALERT: {@event.Symbol} {direction} {Math.Abs(percentChange):F2}% to ${@event.NewPrice:F2}");
            }
        }

        protected override void OnStockVolumeUpdated(StockVolumeUpdatedEvent @event)
        {
            // Mobile app shows volume updates only for large volumes
            if (@event.Volume > 2_000_000)
            {
                Console.WriteLine($"[{_name}] High volume alert for {@event.Symbol}: {(@event.Volume / 1_000_000.0):F2}M shares");
            }
        }
    }
}