namespace Cloud_patterns.Events
{
    // Concrete event for stock volume updates
    public class StockVolumeUpdatedEvent : EventBase
    {
        public string Symbol { get; }
        public long Volume { get; }

        public StockVolumeUpdatedEvent(string source, string symbol, long volume) 
            : base(source)
        {
            Symbol = symbol;
            Volume = volume;
        }
    }
}