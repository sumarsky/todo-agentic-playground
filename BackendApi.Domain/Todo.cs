namespace BackendApi.Domain;

public record Todo
{
    public Guid Id { get; private init; }
    public string Title { get; init; }
    public bool Completed { get; init; }
    public DateTime CreatedAt { get; private init; }

    public Todo(Guid id, string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty or null", nameof(title));

        Id = id;
        Title = title;
        Completed = false;
        CreatedAt = DateTime.UtcNow;
    }

    public Todo(Guid id, string title, bool completed, DateTime createdAt)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty or null", nameof(title));

        Id = id;
        Title = title;
        Completed = completed;
        CreatedAt = createdAt;
    }

    public Todo ToggleCompleted() => this with { Completed = !Completed };

    public Todo WithTitle(string newTitle)
    {
        if (string.IsNullOrWhiteSpace(newTitle))
            throw new ArgumentException("Title cannot be empty or null", nameof(newTitle));

        return this with { Title = newTitle };
    }
}
