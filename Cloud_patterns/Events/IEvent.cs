using System;

namespace Cloud_patterns.Events
{
    // Base event interface
    public interface IEvent
    {
        Guid Id { get; }
        DateTime Timestamp { get; }
        string Source { get; }
    }
}