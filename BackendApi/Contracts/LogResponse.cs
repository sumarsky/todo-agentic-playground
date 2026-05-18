namespace BackendApi.Contracts;

public record LogResponse(
    Guid Id,
    DateTime Timestamp,
    string Level,
    string Source,
    string Message,
    string? HttpMethod,
    string? HttpPath,
    int? HttpStatus,
    double? DurationMs,
    string? TraceId,
    string? ExceptionType,
    string? ExceptionMessage
);
