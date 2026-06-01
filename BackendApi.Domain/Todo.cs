namespace BackendApi.Domain;

public record Todo
{
    public TodoId Id { get; private init; }
    public TodoTitle Title { get; init; }
    public bool Completed { get; init; }
    public DateTimeOffset CreatedAt { get; private init; }

    public Todo(TodoId id, TodoTitle title)
    {
        Id = id;
        Title = title;
        Completed = false;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public Todo ToggleCompleted() => this with { Completed = !Completed };

    public Todo WithTitle(TodoTitle newTitle) => this with { Title = newTitle };
}
