using BackendApi.Contracts;
using BackendApi.Domain;

namespace BackendApi.Mappers;

public static class LogMapper
{
    public static LogResponse ToResponse(LogEntry entry)
    {
        return new LogResponse(
            Id: entry.Id,
            Timestamp: entry.Timestamp,
            Level: entry.Level,
            Source: entry.Source,
            Message: entry.Message,
            HttpMethod: entry.HttpMethod,
            HttpPath: entry.HttpPath,
            HttpStatus: entry.HttpStatus,
            DurationMs: entry.DurationMs,
            TraceId: entry.TraceId,
            ExceptionType: entry.ExceptionType,
            ExceptionMessage: entry.ExceptionMessage
        );
    }

    public static IReadOnlyList<LogResponse> ToResponses(IReadOnlyList<LogEntry> entries)
    {
        return entries.Select(ToResponse).ToList().AsReadOnly();
    }
}
