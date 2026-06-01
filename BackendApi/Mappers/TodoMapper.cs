using BackendApi.Contracts;
using BackendApi.Domain;

namespace BackendApi.Mappers;

public static class TodoMapper
{
    public static TodoResponse ToResponse(Todo todo)
    {
        return new TodoResponse(
            Id: todo.Id,
            Title: todo.Title,
            Completed: todo.Completed,
            CreatedAt: todo.CreatedAt.UtcDateTime
        );
    }

    public static IReadOnlyList<TodoResponse> ToResponses(IReadOnlyList<Todo> todos)
    {
        return todos.Select(ToResponse).ToList().AsReadOnly();
    }
}
