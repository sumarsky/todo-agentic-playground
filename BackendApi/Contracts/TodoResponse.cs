namespace BackendApi.Contracts;

public record TodoResponse(Guid Id, string Title, bool Completed, DateTime CreatedAt);
