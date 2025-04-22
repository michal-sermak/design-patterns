namespace Cloud_patterns.CQRS.Write
{
    // Data structure for the write model
    public class StockWriteData
    {
        public string Symbol { get; set; } = "";
        public decimal Price { get; set; }
        public long Volume { get; set; }
    }
}