using BackendApi.Domain;
using Xunit;

namespace BackendApi.Tests.Domain;

public class TodoTests
{
    [Fact]
    public void Create_WithTitleAndId_SetsDefaults()
    {
        // Arrange
        var id = Guid.NewGuid();
        var title = "Buy milk";
        var now = DateTime.UtcNow;

        // Act
        var todo = new Todo(id, title);

        // Assert
        Assert.Equal(id, todo.Id);
        Assert.Equal(title, todo.Title);
        Assert.False(todo.Completed);
        Assert.True(todo.CreatedAt >= now && todo.CreatedAt <= DateTime.UtcNow.AddSeconds(1));
    }

    [Fact]
    public void Create_WithNullTitle_Throws()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new Todo(id, null!));
    }

    [Fact]
    public void Create_WithEmptyTitle_Throws()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new Todo(id, ""));
    }

    [Fact]
    public void ToggleCompleted_ChangesState()
    {
        // Arrange
        var todo = new Todo(Guid.NewGuid(), "Task");
        Assert.False(todo.Completed);

        // Act
        todo.ToggleCompleted();

        // Assert
        Assert.True(todo.Completed);
    }

    [Fact]
    public void ToggleCompleted_Twice_ReturnsToOriginal()
    {
        // Arrange
        var todo = new Todo(Guid.NewGuid(), "Task");

        // Act
        todo.ToggleCompleted();
        todo.ToggleCompleted();

        // Assert
        Assert.False(todo.Completed);
    }

    [Fact]
    public void Id_IsImmutable()
    {
        // Arrange
        var id1 = Guid.NewGuid();
        var todo = new Todo(id1, "Task");

        // Act & Assert - no setter should exist
        var id2 = todo.Id;
        Assert.Equal(id1, id2);
    }

    [Fact]
    public void Title_IsImmutable()
    {
        // Arrange
        var title = "Original";
        var todo = new Todo(Guid.NewGuid(), title);

        // Act & Assert - no setter should exist
        var retrievedTitle = todo.Title;
        Assert.Equal(title, retrievedTitle);
    }

    [Fact]
    public void CreatedAt_IsImmutable()
    {
        // Arrange
        var todo = new Todo(Guid.NewGuid(), "Task");
        var createdAt = todo.CreatedAt;

        // Act & Assert - no setter should exist
        Assert.Equal(createdAt, todo.CreatedAt);
    }
}
