using BackendApi.Domain;
using BackendApi.Storage.Postgres;
using Dapper;
using Npgsql;
using Testcontainers.PostgreSql;

namespace BackendApi.Tests.Storage.Postgres;

public class PostgresTodoRepositoryTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer = new PostgreSqlBuilder("postgres:latest")
        .WithDatabase("testdb")
        .WithUsername("test")
        .WithPassword("test")
        .Build();

    private NpgsqlDataSource? _dataSource;
    private PostgresTodoRepository? _repository;

    private static Todo NewTodo(string title) =>
        new(
            TodoId.New(),
            new TodoTitle(title),
            completed: false,
            new DateTimeOffset(2026, 1, 2, 3, 4, 5, TimeSpan.Zero).AddTicks(1234560));

    public async Task InitializeAsync()
    {
        await _postgresContainer.StartAsync();

        var connectionString = _postgresContainer.GetConnectionString();
        _dataSource = NpgsqlDataSource.Create(connectionString);

        await using var conn = await _dataSource.OpenConnectionAsync();
        await conn.ExecuteAsync(@"
            CREATE TABLE IF NOT EXISTS todos (
                id UUID PRIMARY KEY,
                title TEXT NOT NULL,
                completed BOOLEAN NOT NULL DEFAULT false,
                created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
            )");

        _repository = new PostgresTodoRepository(_dataSource);
    }

    public async Task DisposeAsync()
    {
        if (_dataSource != null)
        {
            await _dataSource.DisposeAsync();
        }
        await _postgresContainer.StopAsync();
    }

    [Fact]
    public async Task AddAsync_InsertsTodo_CanRetrieveById()
    {
        // Arrange
        var todo = NewTodo("Test todo");

        // Act
        await _repository!.AddAsync(todo);
        var retrieved = await _repository.GetByIdAsync(todo.Id);

        // Assert
        Assert.NotNull(retrieved);
        Assert.Equal(todo.Id, retrieved.Id);
        Assert.Equal(todo.Title, retrieved.Title);
        Assert.False(retrieved.Completed);
        Assert.Equal(todo.CreatedAt, retrieved.CreatedAt);
        Assert.Equal(TimeSpan.Zero, retrieved.CreatedAt.Offset);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistentId_ReturnsNull()
    {
        // Act
        var result = await _repository!.GetByIdAsync(TodoId.New());

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllAsync_NoFilters_ReturnsAllTodos()
    {
        // Arrange
        var todo1 = NewTodo("Todo 1");
        var todo2 = NewTodo("Todo 2");
        var todo3 = NewTodo("Todo 3");

        await _repository!.AddAsync(todo1);
        await _repository.AddAsync(todo2);
        await _repository.AddAsync(todo3);

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        Assert.Equal(3, result.Count);
    }

    [Fact]
    public async Task GetAllAsync_FilterCompleted_ReturnsOnlyCompletedTodos()
    {
        // Arrange
        var todo1 = NewTodo("Task 1");
        var todo2 = NewTodo("Task 2");
        var todo3 = NewTodo("Task 3");

        await _repository!.AddAsync(todo1);
        await _repository.AddAsync(todo2);
        await _repository.AddAsync(todo3);

        var completed = todo1.ToggleCompleted();
        await using var conn = await _dataSource!.OpenConnectionAsync();
        await conn.ExecuteAsync("UPDATE todos SET completed = @Completed WHERE id = @Id",
            new { completed.Completed, Id = completed.Id.Value });

        // Act
        var result = await _repository.GetAllAsync(completed: true);

        // Assert
        Assert.Single(result);
        Assert.True(result.First().Completed);
    }

    [Fact]
    public async Task GetAllAsync_SearchByTitle_ReturnsMatchingTodos()
    {
        // Arrange
        var todo1 = NewTodo("Buy groceries");
        var todo2 = NewTodo("Buy milk");
        var todo3 = NewTodo("Walk dog");

        await _repository!.AddAsync(todo1);
        await _repository.AddAsync(todo2);
        await _repository.AddAsync(todo3);

        // Act
        var result = await _repository.GetAllAsync(search: "Buy");

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, t => Assert.Contains("Buy", t.Title));
    }

    [Fact]
    public async Task GetAllAsync_SearchCaseInsensitive_ReturnsMatchingTodos()
    {
        // Arrange
        var todo1 = NewTodo("Buy Groceries");
        var todo2 = NewTodo("Walk dog");

        await _repository!.AddAsync(todo1);
        await _repository.AddAsync(todo2);

        // Act
        var result = await _repository.GetAllAsync(search: "buy");

        // Assert
        Assert.Single(result);
        Assert.Equal("Buy Groceries", result.First().Title);
    }

    [Fact]
    public async Task GetAllAsync_CombineCompletedAndSearch_ReturnsCorrectResults()
    {
        // Arrange
        var todo1 = NewTodo("Buy milk");
        var todo2 = NewTodo("Buy groceries");
        var todo3 = NewTodo("Walk dog");

        await _repository!.AddAsync(todo1);
        await _repository.AddAsync(todo2);
        await _repository.AddAsync(todo3);

        var completed1 = todo1.ToggleCompleted();
        var completed2 = todo2.ToggleCompleted();
        await using var conn = await _dataSource!.OpenConnectionAsync();
        await conn.ExecuteAsync("UPDATE todos SET completed = @Completed WHERE id = @Id",
            new { completed1.Completed, Id = completed1.Id.Value });
        await conn.ExecuteAsync("UPDATE todos SET completed = @Completed WHERE id = @Id",
            new { completed2.Completed, Id = completed2.Id.Value });

        // Act
        var result = await _repository.GetAllAsync(completed: true, search: "Buy");

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, t => Assert.True(t.Completed));
        Assert.All(result, t => Assert.Contains("Buy", t.Title));
    }

    [Fact]
    public async Task DeleteAsync_RemovesTodoAndPreservesOthers()
    {
        // Arrange
        var todo1 = NewTodo("Todo 1");
        var todo2 = NewTodo("Todo 2");
        var todo3 = NewTodo("Todo 3");

        await _repository!.AddAsync(todo1);
        await _repository.AddAsync(todo2);
        await _repository.AddAsync(todo3);

        // Act
        await _repository.DeleteAsync(todo2.Id);

        // Assert
        Assert.NotNull(await _repository.GetByIdAsync(todo1.Id));
        Assert.Null(await _repository.GetByIdAsync(todo2.Id));
        Assert.NotNull(await _repository.GetByIdAsync(todo3.Id));
    }

    [Fact]
    public async Task DeleteByIdsAsync_RemovesMatchingTodosAndPreservesOthers()
    {
        // Arrange
        var todo1 = NewTodo("Todo 1");
        var todo2 = NewTodo("Todo 2");
        var todo3 = NewTodo("Todo 3");
        var todo4 = NewTodo("Todo 4");

        await _repository!.AddAsync(todo1);
        await _repository.AddAsync(todo2);
        await _repository.AddAsync(todo3);
        await _repository.AddAsync(todo4);

        // Act
        await _repository.DeleteByIdsAsync(new[] { todo1.Id, todo3.Id });

        // Assert
        Assert.Null(await _repository.GetByIdAsync(todo1.Id));
        Assert.NotNull(await _repository.GetByIdAsync(todo2.Id));
        Assert.Null(await _repository.GetByIdAsync(todo3.Id));
        Assert.NotNull(await _repository.GetByIdAsync(todo4.Id));
    }
}
