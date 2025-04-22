using System;

namespace Cloud_patterns.CQRS.Read
{
    // Data structure for the read model - optimized for queries
    public class StockReadData
    {
        public string Symbol { get; set; } = "";
        public decimal CurrentPrice { get; set; }
        public decimal PriceChange { get; set; }
        public decimal PriceChangePercent { get; set; }
        public long Volume { get; set; }
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }
}