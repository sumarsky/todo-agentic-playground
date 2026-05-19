namespace BackendApi.Application;

public record LogFilter
{
    public string? Level { get; init; }
    public string? Message { get; init; }
    public DateTimeOffset? Since { get; init; }
}
