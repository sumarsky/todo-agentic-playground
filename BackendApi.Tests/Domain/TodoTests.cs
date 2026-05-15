using BackendApi.Domain;
using Xunit;

namespace BackendApi.Tests.Domain;

public class TodoTests
{
    [Fact]
    public void Create_WithTitleAndId_SetsDefaults()
    {
        var id = Guid.NewGuid();
        var title = "Buy milk";
        var now = DateTime.UtcNow;

        var todo = new Todo(id, title);

        Assert.Equal(id, todo.Id);
        Assert.Equal(title, todo.Title);
        Assert.False(todo.Completed);
        Assert.True(todo.CreatedAt >= now && todo.CreatedAt <= DateTime.UtcNow.AddSeconds(1));
    }

    [Fact]
    public void Create_WithNullTitle_Throws()
    {
        var id = Guid.NewGuid();

        Assert.Throws<ArgumentException>(() => new Todo(id, null!));
    }

    [Fact]
    public void Create_WithEmptyTitle_Throws()
    {
        var id = Guid.NewGuid();

        Assert.Throws<ArgumentException>(() => new Todo(id, ""));
    }

    [Fact]
    public void ToggleCompleted_ReturnsNewInstanceWithFlippedState()
    {
        var todo = new Todo(Guid.NewGuid(), "Task");
        Assert.False(todo.Completed);

        var toggled = todo.ToggleCompleted();

        Assert.True(toggled.Completed);
        Assert.False(todo.Completed);
        Assert.NotSame(todo, toggled);
    }

    [Fact]
    public void ToggleCompleted_Twice_ReturnsToOriginal()
    {
        var todo = new Todo(Guid.NewGuid(), "Task");

        var toggled = todo.ToggleCompleted();
        var toggledAgain = toggled.ToggleCompleted();

        Assert.False(toggledAgain.Completed);
        Assert.NotSame(todo, toggledAgain);
    }

    [Fact]
    public void WithTitle_ReturnsNewInstanceWithUpdatedTitle()
    {
        var todo = new Todo(Guid.NewGuid(), "Original");

        var updated = todo.WithTitle("Updated");

        Assert.Equal("Updated", updated.Title);
        Assert.Equal("Original", todo.Title);
        Assert.NotSame(todo, updated);
    }

    [Fact]
    public void WithTitle_PreservesOtherFields()
    {
        var id = Guid.NewGuid();
        var original = new Todo(id, "Original");
        var toggled = original.ToggleCompleted();

        var updated = toggled.WithTitle("Updated");

        Assert.Equal(id, updated.Id);
        Assert.True(updated.Completed);
        Assert.Equal(original.CreatedAt, updated.CreatedAt);
    }

    [Fact]
    public void WithTitle_WithEmptyTitle_Throws()
    {
        var todo = new Todo(Guid.NewGuid(), "Task");

        Assert.Throws<ArgumentException>(() => todo.WithTitle(""));
    }

    [Fact]
    public void Id_IsImmutable()
    {
        var id1 = Guid.NewGuid();
        var todo = new Todo(id1, "Task");

        var id2 = todo.Id;
        Assert.Equal(id1, id2);
    }

    [Fact]
    public void CreatedAt_IsImmutable()
    {
        var todo = new Todo(Guid.NewGuid(), "Task");
        var createdAt = todo.CreatedAt;

        Assert.Equal(createdAt, todo.CreatedAt);
    }
}
