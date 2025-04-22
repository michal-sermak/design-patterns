namespace Cloud_patterns.Events
{
    // Concrete event for stock price updates
    public class StockPriceUpdatedEvent : EventBase
    {
        public string Symbol { get; }
        public decimal PreviousPrice { get; }
        public decimal NewPrice { get; }

        public StockPriceUpdatedEvent(string source, string symbol, decimal previousPrice, decimal newPrice) 
            : base(source)
        {
            Symbol = symbol;
            PreviousPrice = previousPrice;
            NewPrice = newPrice;
        }
    }
}