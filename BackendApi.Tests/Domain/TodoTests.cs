using BackendApi.Domain;
using Xunit;

namespace BackendApi.Tests.Domain;

public class TodoTests
{
    [Fact]
    public void Create_WithTitleAndId_SetsDefaults()
    {
        var id = TodoId.New();
        var title = new TodoTitle("Buy milk");
        var now = DateTimeOffset.UtcNow;

        var todo = new Todo(id, title);

        Assert.Equal(id, todo.Id);
        Assert.Equal(title, todo.Title);
        Assert.False(todo.Completed);
        Assert.True(todo.CreatedAt >= now && todo.CreatedAt <= DateTimeOffset.UtcNow.AddSeconds(1));
    }

    [Fact]
    public void Create_WithNullTitle_Throws()
    {
        Assert.Throws<ArgumentException>(() => new TodoTitle(null!));
    }

    [Fact]
    public void Create_WithEmptyTitle_Throws()
    {
        Assert.Throws<ArgumentException>(() => new TodoTitle(""));
    }

    [Fact]
    public void ToggleCompleted_ReturnsNewInstanceWithFlippedState()
    {
        var todo = new Todo(TodoId.New(), new TodoTitle("Task"));
        Assert.False(todo.Completed);

        var toggled = todo.ToggleCompleted();

        Assert.True(toggled.Completed);
        Assert.False(todo.Completed);
        Assert.NotSame(todo, toggled);
    }

    [Fact]
    public void ToggleCompleted_Twice_ReturnsToOriginal()
    {
        var todo = new Todo(TodoId.New(), new TodoTitle("Task"));

        var toggled = todo.ToggleCompleted();
        var toggledAgain = toggled.ToggleCompleted();

        Assert.False(toggledAgain.Completed);
        Assert.NotSame(todo, toggledAgain);
    }

    [Fact]
    public void WithTitle_ReturnsNewInstanceWithUpdatedTitle()
    {
        var todo = new Todo(TodoId.New(), new TodoTitle("Original"));

        var updated = todo.WithTitle(new TodoTitle("Updated"));

        Assert.Equal(new TodoTitle("Updated"), updated.Title);
        Assert.Equal(new TodoTitle("Original"), todo.Title);
        Assert.NotSame(todo, updated);
    }

    [Fact]
    public void WithTitle_PreservesOtherFields()
    {
        var id = TodoId.New();
        var original = new Todo(id, new TodoTitle("Original"));
        var toggled = original.ToggleCompleted();

        var updated = toggled.WithTitle(new TodoTitle("Updated"));

        Assert.Equal(id, updated.Id);
        Assert.True(updated.Completed);
        Assert.Equal(original.CreatedAt, updated.CreatedAt);
    }

    [Fact]
    public void WithTitle_WithEmptyTitle_Throws()
    {
        Assert.Throws<ArgumentException>(() => new TodoTitle(""));
    }

    [Fact]
    public void Id_IsImmutable()
    {
        var id1 = TodoId.New();
        var todo = new Todo(id1, new TodoTitle("Task"));

        var id2 = todo.Id;
        Assert.Equal(id1, id2);
    }

    [Fact]
    public void CreatedAt_IsImmutable()
    {
        var todo = new Todo(TodoId.New(), new TodoTitle("Task"));
        var createdAt = todo.CreatedAt;

        Assert.Equal(createdAt, todo.CreatedAt);
    }
}
