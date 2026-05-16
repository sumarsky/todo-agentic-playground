using BackendApi.Domain;
using BackendApi.Infrastructure;
using Xunit;

namespace BackendApi.Tests.Infrastructure;

public class InMemoryTodoRepositoryTests
{
    [Fact]
    public async Task AddAsync_StoresTodo_CanRetrieveById()
    {
        var repo = new InMemoryTodoRepository();
        var id = Guid.NewGuid();
        var todo = new Todo(id, "Test task");

        await repo.AddAsync(todo);
        var retrieved = await repo.GetByIdAsync(id);

        Assert.NotNull(retrieved);
        Assert.Equal(id, retrieved.Id);
        Assert.Equal("Test task", retrieved.Title);
    }

    [Fact]
    public async Task GetByIdAsync_WithNonexistentId_ReturnsNull()
    {
        var repo = new InMemoryTodoRepository();
        var nonexistentId = Guid.NewGuid();

        var retrieved = await repo.GetByIdAsync(nonexistentId);

        Assert.Null(retrieved);
    }

    [Fact]
    public async Task GetAllAsync_WithNoTodos_ReturnsEmpty()
    {
        var repo = new InMemoryTodoRepository();

        var todos = await repo.GetAllAsync();

        Assert.Empty(todos);
    }

    [Fact]
    public async Task GetAllAsync_WithMultipleTodos_ReturnsAll()
    {
        var repo = new InMemoryTodoRepository();
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();
        var todo1 = new Todo(id1, "Task 1");
        var todo2 = new Todo(id2, "Task 2");

        await repo.AddAsync(todo1);
        await repo.AddAsync(todo2);
        var todos = await repo.GetAllAsync();

        Assert.Equal(2, todos.Count);
        Assert.Contains(todos, t => t.Id == id1);
        Assert.Contains(todos, t => t.Id == id2);
    }

    [Fact]
    public async Task UpdateAsync_ReplacesExistingTodo()
    {
        var repo = new InMemoryTodoRepository();
        var id = Guid.NewGuid();
        var todo = new Todo(id, "Original");
        await repo.AddAsync(todo);
        var toggled = todo.ToggleCompleted();

        await repo.UpdateAsync(toggled);
        var retrieved = await repo.GetByIdAsync(id);

        Assert.NotNull(retrieved);
        Assert.True(retrieved.Completed);
    }

    [Fact]
    public async Task UpdateAsync_WithNonexistentId_DoesNotThrow()
    {
        var repo = new InMemoryTodoRepository();
        var id = Guid.NewGuid();
        var todo = new Todo(id, "Nonexistent");

        await repo.UpdateAsync(todo);
    }

    [Fact]
    public async Task DeleteAsync_RemovesTodo()
    {
        var repo = new InMemoryTodoRepository();
        var id = Guid.NewGuid();
        var todo = new Todo(id, "Task");
        await repo.AddAsync(todo);

        await repo.DeleteAsync(id);
        var retrieved = await repo.GetByIdAsync(id);

        Assert.Null(retrieved);
    }

    [Fact]
    public async Task DeleteAsync_WithNonexistentId_DoesNotThrow()
    {
        var repo = new InMemoryTodoRepository();
        var id = Guid.NewGuid();

        await repo.DeleteAsync(id);
    }

    [Fact]
    public async Task DeleteByIdsAsync_RemovesMultipleTodos()
    {
        var repo = new InMemoryTodoRepository();
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();
        var id3 = Guid.NewGuid();
        await repo.AddAsync(new Todo(id1, "Task 1"));
        await repo.AddAsync(new Todo(id2, "Task 2"));
        await repo.AddAsync(new Todo(id3, "Task 3"));

        await repo.DeleteByIdsAsync(new[] { id1, id2 });

        Assert.Null(await repo.GetByIdAsync(id1));
        Assert.Null(await repo.GetByIdAsync(id2));
        Assert.NotNull(await repo.GetByIdAsync(id3));
    }

    [Fact]
    public async Task DeleteByIdsAsync_WithEmptyList_DoesNotThrow()
    {
        var repo = new InMemoryTodoRepository();
        var id = Guid.NewGuid();
        await repo.AddAsync(new Todo(id, "Task"));

        await repo.DeleteByIdsAsync(Array.Empty<Guid>());
        Assert.NotNull(await repo.GetByIdAsync(id));
    }

    [Fact]
    public async Task GetAllAsync_FilterCompleted_ReturnsOnlyCompleted()
    {
        var repo = new InMemoryTodoRepository();
        var todo1 = new Todo(Guid.NewGuid(), "Task 1");
        var todo2 = new Todo(Guid.NewGuid(), "Task 2");
        var todo3 = new Todo(Guid.NewGuid(), "Task 3");

        await repo.AddAsync(todo1);
        await repo.AddAsync(todo2);
        await repo.AddAsync(todo3);

        var completed = todo1.ToggleCompleted();
        await repo.UpdateAsync(completed);

        var result = await repo.GetAllAsync(completed: true);

        Assert.Single(result);
        Assert.True(result.First().Completed);
    }

    [Fact]
    public async Task GetAllAsync_SearchByTitle_ReturnsMatching()
    {
        var repo = new InMemoryTodoRepository();
        await repo.AddAsync(new Todo(Guid.NewGuid(), "Buy groceries"));
        await repo.AddAsync(new Todo(Guid.NewGuid(), "Buy milk"));
        await repo.AddAsync(new Todo(Guid.NewGuid(), "Walk dog"));

        var result = await repo.GetAllAsync(search: "Buy");

        Assert.Equal(2, result.Count);
        Assert.All(result, t => Assert.Contains("Buy", t.Title));
    }

    [Fact]
    public async Task ConcurrentAdds_ThreadSafe()
    {
        var repo = new InMemoryTodoRepository();
        var idCount = 100;
        var ids = Enumerable.Range(0, idCount).Select(_ => Guid.NewGuid()).ToList();

        await Task.WhenAll(ids.Select(id => Task.Run(() => repo.AddAsync(new Todo(id, "Concurrent task")))));

        var all = await repo.GetAllAsync();
        Assert.Equal(idCount, all.Count);
        foreach (var id in ids)
        {
            Assert.NotNull(await repo.GetByIdAsync(id));
        }
    }
}
