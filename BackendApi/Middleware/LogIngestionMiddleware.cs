using System.Diagnostics;
using BackendApi.Application.Ports;
using BackendApi.Domain;
using Microsoft.AspNetCore.Http;

namespace BackendApi.Middleware;

public class LogIngestionMiddleware
{
    private readonly RequestDelegate _next;

    public LogIngestionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ILogStore logStore)
    {
        var stopwatch = Stopwatch.StartNew();

        await _next(context);

        stopwatch.Stop();

        var method = context.Request.Method;
        var path = context.Request.Path.ToString();
        var status = context.Response.StatusCode;
        var durationMs = stopwatch.Elapsed.TotalMilliseconds;
        var traceId = Activity.Current?.TraceId.ToString();

        var level = status switch
        {
            >= 200 and <= 399 => "info",
            >= 400 and <= 499 => "warning",
            _ => "error"
        };
        var message = $"{method} {path} returned {status}";

        var entry = new LogEntry(Guid.NewGuid(), DateTime.UtcNow, level, "middleware", message)
        {
            HttpMethod = method,
            HttpPath = path,
            HttpStatus = status,
            DurationMs = durationMs,
            TraceId = traceId
        };

        await logStore.WriteAsync(entry);
    }
}
