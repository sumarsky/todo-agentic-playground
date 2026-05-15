using BackendApi.Domain;
using BackendApi.Infrastructure;
using Xunit;

namespace BackendApi.Tests.Infrastructure;

public class InMemoryTodoRepositoryTests
{
    [Fact]
    public void Add_StoresTodo_CanRetrieveById()
    {
        var repo = new InMemoryTodoRepository();
        var id = Guid.NewGuid();
        var todo = new Todo(id, "Test task");

        repo.Add(todo);
        var retrieved = repo.GetById(id);

        Assert.NotNull(retrieved);
        Assert.Equal(id, retrieved.Id);
        Assert.Equal("Test task", retrieved.Title);
    }

    [Fact]
    public void GetById_WithNonexistentId_ReturnsNull()
    {
        var repo = new InMemoryTodoRepository();
        var nonexistentId = Guid.NewGuid();

        var retrieved = repo.GetById(nonexistentId);

        Assert.Null(retrieved);
    }

    [Fact]
    public void GetAll_WithNoTodos_ReturnsEmpty()
    {
        var repo = new InMemoryTodoRepository();

        var todos = repo.GetAll();

        Assert.Empty(todos);
    }

    [Fact]
    public void GetAll_WithMultipleTodos_ReturnsAll()
    {
        var repo = new InMemoryTodoRepository();
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();
        var todo1 = new Todo(id1, "Task 1");
        var todo2 = new Todo(id2, "Task 2");

        repo.Add(todo1);
        repo.Add(todo2);
        var todos = repo.GetAll();

        Assert.Equal(2, todos.Count);
        Assert.Contains(todos, t => t.Id == id1);
        Assert.Contains(todos, t => t.Id == id2);
    }

    [Fact]
    public void Update_ReplacesExistingTodo()
    {
        var repo = new InMemoryTodoRepository();
        var id = Guid.NewGuid();
        var todo = new Todo(id, "Original");
        repo.Add(todo);
        var toggled = todo.ToggleCompleted();

        repo.Update(toggled);
        var retrieved = repo.GetById(id);

        Assert.NotNull(retrieved);
        Assert.True(retrieved.Completed);
    }

    [Fact]
    public void Update_WithNonexistentId_DoesNotThrow()
    {
        var repo = new InMemoryTodoRepository();
        var id = Guid.NewGuid();
        var todo = new Todo(id, "Nonexistent");

        repo.Update(todo);
    }

    [Fact]
    public void Delete_RemovesTodo()
    {
        var repo = new InMemoryTodoRepository();
        var id = Guid.NewGuid();
        var todo = new Todo(id, "Task");
        repo.Add(todo);

        repo.Delete(id);
        var retrieved = repo.GetById(id);

        Assert.Null(retrieved);
    }

    [Fact]
    public void Delete_WithNonexistentId_DoesNotThrow()
    {
        var repo = new InMemoryTodoRepository();
        var id = Guid.NewGuid();

        repo.Delete(id);
    }

    [Fact]
    public void DeleteByIds_RemovesMultipleTodos()
    {
        var repo = new InMemoryTodoRepository();
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();
        var id3 = Guid.NewGuid();
        repo.Add(new Todo(id1, "Task 1"));
        repo.Add(new Todo(id2, "Task 2"));
        repo.Add(new Todo(id3, "Task 3"));

        repo.DeleteByIds(new[] { id1, id2 });

        Assert.Null(repo.GetById(id1));
        Assert.Null(repo.GetById(id2));
        Assert.NotNull(repo.GetById(id3));
    }

    [Fact]
    public void DeleteByIds_WithEmptyList_DoesNotThrow()
    {
        var repo = new InMemoryTodoRepository();
        var id = Guid.NewGuid();
        repo.Add(new Todo(id, "Task"));

        repo.DeleteByIds(Array.Empty<Guid>());
        Assert.NotNull(repo.GetById(id));
    }

    [Fact]
    public async Task ConcurrentAdds_ThreadSafe()
    {
        var repo = new InMemoryTodoRepository();
        var idCount = 100;
        var ids = Enumerable.Range(0, idCount).Select(_ => Guid.NewGuid()).ToList();

        await Task.WhenAll(ids.Select(id => Task.Run(() => repo.Add(new Todo(id, "Concurrent task")))));

        var all = repo.GetAll();
        Assert.Equal(idCount, all.Count);
        foreach (var id in ids)
        {
            Assert.NotNull(repo.GetById(id));
        }
    }
}
