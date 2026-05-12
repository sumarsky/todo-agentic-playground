namespace BackendApi.Domain;

public class Todo
{
    public Guid Id { get; }
    public string Title { get; }
    public bool Completed { get; private set; }
    public DateTime CreatedAt { get; }

    public Todo(Guid id, string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty or null", nameof(title));

        Id = id;
        Title = title;
        Completed = false;
        CreatedAt = DateTime.UtcNow;
    }

    public void ToggleCompleted()
    {
        Completed = !Completed;
    }
}
