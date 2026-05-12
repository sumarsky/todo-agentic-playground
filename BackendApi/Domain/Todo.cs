namespace BackendApi.Domain;

public class Todo
{
    public Guid Id { get; }
    public string Title { get; private set; }
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

    public void UpdateTitle(string newTitle)
    {
        if (string.IsNullOrWhiteSpace(newTitle))
            throw new ArgumentException("Title cannot be empty or null", nameof(newTitle));

        Title = newTitle;
    }
}
