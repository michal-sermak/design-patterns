using System;
using System.Threading.Tasks;
using Cloud_patterns.Events;
using Cloud_patterns.PubSub;

namespace Cloud_patterns.Subscribers
{
    // Base class for all subscribers
    public abstract class SubscriberBase
    {
        protected readonly string _name;
        protected readonly AzureServiceBusEventBroker _eventBroker;

        public SubscriberBase(string name, AzureServiceBusEventBroker eventBroker)
        {
            _name = name;
            _eventBroker = eventBroker;
            RegisterSubscriptions().GetAwaiter().GetResult();
        }

        private async Task RegisterSubscriptions()
        {
            string clientId = $"{_name}-{Guid.NewGuid().ToString().Substring(0, 8)}";
            await _eventBroker.SubscribeAsync<StockPriceUpdatedEvent>(clientId, 
                async evt => { OnStockPriceUpdated(evt); await Task.CompletedTask; });
            
            await _eventBroker.SubscribeAsync<StockVolumeUpdatedEvent>(clientId, 
                async evt => { OnStockVolumeUpdated(evt); await Task.CompletedTask; });
        }

        protected virtual void OnStockPriceUpdated(StockPriceUpdatedEvent @event)
        {
            // Default implementation - override in derived classes
        }

        protected virtual void OnStockVolumeUpdated(StockVolumeUpdatedEvent @event)
        {
            // Default implementation - override in derived classes
        }
    }
}