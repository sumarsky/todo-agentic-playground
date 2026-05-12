using BackendApi.Domain;
using BackendApi.Infrastructure;
using Xunit;

namespace BackendApi.Tests.Infrastructure;

public class InMemoryTodoRepositoryTests
{
    [Fact]
    public void Add_StoresTodo_CanRetrieveById()
    {
        // Arrange
        var repo = new InMemoryTodoRepository();
        var id = Guid.NewGuid();
        var todo = new Todo(id, "Test task");

        // Act
        repo.Add(todo);
        var retrieved = repo.GetById(id);

        // Assert
        Assert.NotNull(retrieved);
        Assert.Equal(id, retrieved.Id);
        Assert.Equal("Test task", retrieved.Title);
    }

    [Fact]
    public void GetById_WithNonexistentId_ReturnsNull()
    {
        // Arrange
        var repo = new InMemoryTodoRepository();
        var nonexistentId = Guid.NewGuid();

        // Act
        var retrieved = repo.GetById(nonexistentId);

        // Assert
        Assert.Null(retrieved);
    }

    [Fact]
    public void GetAll_WithNoTodos_ReturnsEmpty()
    {
        // Arrange
        var repo = new InMemoryTodoRepository();

        // Act
        var todos = repo.GetAll();

        // Assert
        Assert.Empty(todos);
    }

    [Fact]
    public void GetAll_WithMultipleTodos_ReturnsAll()
    {
        // Arrange
        var repo = new InMemoryTodoRepository();
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();
        var todo1 = new Todo(id1, "Task 1");
        var todo2 = new Todo(id2, "Task 2");

        // Act
        repo.Add(todo1);
        repo.Add(todo2);
        var todos = repo.GetAll();

        // Assert
        Assert.Equal(2, todos.Count);
        Assert.Contains(todos, t => t.Id == id1);
        Assert.Contains(todos, t => t.Id == id2);
    }

    [Fact]
    public void Update_ModifiesExistingTodo()
    {
        // Arrange
        var repo = new InMemoryTodoRepository();
        var id = Guid.NewGuid();
        var todo = new Todo(id, "Original");
        repo.Add(todo);
        todo.ToggleCompleted();

        // Act
        repo.Update(todo);
        var retrieved = repo.GetById(id);

        // Assert
        Assert.NotNull(retrieved);
        Assert.True(retrieved.Completed);
    }

    [Fact]
    public void Update_WithNonexistentId_DoesNotThrow()
    {
        // Arrange
        var repo = new InMemoryTodoRepository();
        var id = Guid.NewGuid();
        var todo = new Todo(id, "Nonexistent");

        // Act & Assert
        repo.Update(todo);
    }

    [Fact]
    public void Delete_RemovesTodo()
    {
        // Arrange
        var repo = new InMemoryTodoRepository();
        var id = Guid.NewGuid();
        var todo = new Todo(id, "Task");
        repo.Add(todo);

        // Act
        repo.Delete(id);
        var retrieved = repo.GetById(id);

        // Assert
        Assert.Null(retrieved);
    }

    [Fact]
    public void Delete_WithNonexistentId_DoesNotThrow()
    {
        // Arrange
        var repo = new InMemoryTodoRepository();
        var id = Guid.NewGuid();

        // Act & Assert
        repo.Delete(id);
    }

    [Fact]
    public void DeleteByIds_RemovesMultipleTodos()
    {
        // Arrange
        var repo = new InMemoryTodoRepository();
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();
        var id3 = Guid.NewGuid();
        repo.Add(new Todo(id1, "Task 1"));
        repo.Add(new Todo(id2, "Task 2"));
        repo.Add(new Todo(id3, "Task 3"));

        // Act
        repo.DeleteByIds(new[] { id1, id2 });

        // Assert
        Assert.Null(repo.GetById(id1));
        Assert.Null(repo.GetById(id2));
        Assert.NotNull(repo.GetById(id3));
    }

    [Fact]
    public void DeleteByIds_WithEmptyList_DoesNotThrow()
    {
        // Arrange
        var repo = new InMemoryTodoRepository();
        var id = Guid.NewGuid();
        repo.Add(new Todo(id, "Task"));

        // Act & Assert
        repo.DeleteByIds(Array.Empty<Guid>());
        Assert.NotNull(repo.GetById(id));
    }

    [Fact]
    public async Task ConcurrentAdds_ThreadSafe()
    {
        // Arrange
        var repo = new InMemoryTodoRepository();
        var idCount = 100;
        var ids = Enumerable.Range(0, idCount).Select(_ => Guid.NewGuid()).ToList();

        // Act - spawn concurrent adds
        await Task.WhenAll(ids.Select(id => Task.Run(() => repo.Add(new Todo(id, "Concurrent task")))));

        // Assert
        var all = repo.GetAll();
        Assert.Equal(idCount, all.Count);
        foreach (var id in ids)
        {
            Assert.NotNull(repo.GetById(id));
        }
    }
}
