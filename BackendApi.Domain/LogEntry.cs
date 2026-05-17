namespace BackendApi.Domain;

public record LogEntry
{
    public Guid Id { get; private init; }
    public DateTime Timestamp { get; private init; }
    public string Level { get; init; }
    public string Source { get; init; }
    public string Message { get; init; }
    public string? HttpMethod { get; init; }
    public string? HttpPath { get; init; }
    public int? HttpStatus { get; init; }
    public double? DurationMs { get; init; }
    public string? TraceId { get; init; }
    public string? ExceptionType { get; init; }
    public string? ExceptionMessage { get; init; }

    public LogEntry(Guid id, DateTime timestamp, string level, string source, string message)
    {
        if (string.IsNullOrWhiteSpace(level))
            throw new ArgumentException("Level cannot be empty", nameof(level));
        if (string.IsNullOrWhiteSpace(source))
            throw new ArgumentException("Source cannot be empty", nameof(source));
        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("Message cannot be empty", nameof(message));

        Id = id;
        Timestamp = timestamp;
        Level = level;
        Source = source;
        Message = message;
    }

    // Dapper materialization constructor
    public LogEntry(
        Guid id,
        DateTime timestamp,
        string level,
        string source,
        string message,
        string? http_method,
        string? http_path,
        int? http_status,
        double? duration_ms,
        string? trace_id,
        string? exception_type,
        string? exception_message)
    {
        Id = id;
        Timestamp = timestamp;
        Level = level;
        Source = source;
        Message = message;
        HttpMethod = http_method;
        HttpPath = http_path;
        HttpStatus = http_status;
        DurationMs = duration_ms;
        TraceId = trace_id;
        ExceptionType = exception_type;
        ExceptionMessage = exception_message;
    }
}
