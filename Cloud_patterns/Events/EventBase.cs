using System;

namespace Cloud_patterns.Events
{
    // Base event implementation
    public abstract class EventBase : IEvent
    {
        public Guid Id { get; } = Guid.NewGuid();
        public DateTime Timestamp { get; } = DateTime.UtcNow;
        public string Source { get; private set; }

        protected EventBase(string source)
        {
            Source = source;
        }
    }
}