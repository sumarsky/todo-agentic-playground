namespace BackendApi.Application.DTOs;

public record TodoResponse(Guid Id, string Title, bool Completed, DateTime CreatedAt);
