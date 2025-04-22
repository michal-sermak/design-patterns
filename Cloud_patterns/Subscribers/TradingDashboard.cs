using System;
using Cloud_patterns.Events;
using Cloud_patterns.PubSub;

namespace Cloud_patterns.Subscribers
{
    // Trading dashboard subscriber
    public class TradingDashboard : SubscriberBase
    {
        public TradingDashboard(string name, AzureServiceBusEventBroker eventBroker) : base(name, eventBroker)
        {
        }

        protected override void OnStockPriceUpdated(StockPriceUpdatedEvent @event)
        {
            var changeAmount = @event.NewPrice - @event.PreviousPrice;
            var changePercent = changeAmount / @event.PreviousPrice * 100;
            
            Console.WriteLine($"[{_name}] {@event.Symbol} price updated: ${@event.NewPrice:F2} ({(changeAmount >= 0 ? "+" : "")}{changeAmount:F2}, {(changePercent >= 0 ? "+" : "")}{changePercent:F2}%)");
        }

        protected override void OnStockVolumeUpdated(StockVolumeUpdatedEvent @event)
        {
            Console.WriteLine($"[{_name}] {@event.Symbol} volume updated: {(@event.Volume / 1_000.0):F0}k shares");
        }
    }
}