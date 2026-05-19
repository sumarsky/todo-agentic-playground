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
                created_at TIMESTAMP NOT NULL DEFAULT NOW()
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
        var todo = new Todo(Guid.NewGuid(), "Test todo");

        // Act
        await _repository!.AddAsync(todo);
        var retrieved = await _repository.GetByIdAsync(todo.Id);

        // Assert
        Assert.NotNull(retrieved);
        Assert.Equal(todo.Id, retrieved.Id);
        Assert.Equal(todo.Title, retrieved.Title);
        Assert.False(retrieved.Completed);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistentId_ReturnsNull()
    {
        // Act
        var result = await _repository!.GetByIdAsync(Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllAsync_NoFilters_ReturnsAllTodos()
    {
        // Arrange
        var todo1 = new Todo(Guid.NewGuid(), "Todo 1");
        var todo2 = new Todo(Guid.NewGuid(), "Todo 2");
        var todo3 = new Todo(Guid.NewGuid(), "Todo 3");

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
        var todo1 = new Todo(Guid.NewGuid(), "Task 1");
        var todo2 = new Todo(Guid.NewGuid(), "Task 2");
        var todo3 = new Todo(Guid.NewGuid(), "Task 3");

        await _repository!.AddAsync(todo1);
        await _repository.AddAsync(todo2);
        await _repository.AddAsync(todo3);

        var completed = todo1.ToggleCompleted();
        await using var conn = await _dataSource!.OpenConnectionAsync();
        await conn.ExecuteAsync("UPDATE todos SET completed = @Completed WHERE id = @Id",
            new { completed.Completed, Id = completed.Id });

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
        var todo1 = new Todo(Guid.NewGuid(), "Buy groceries");
        var todo2 = new Todo(Guid.NewGuid(), "Buy milk");
        var todo3 = new Todo(Guid.NewGuid(), "Walk dog");

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
        var todo1 = new Todo(Guid.NewGuid(), "Buy Groceries");
        var todo2 = new Todo(Guid.NewGuid(), "Walk dog");

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
        var todo1 = new Todo(Guid.NewGuid(), "Buy milk");
        var todo2 = new Todo(Guid.NewGuid(), "Buy groceries");
        var todo3 = new Todo(Guid.NewGuid(), "Walk dog");

        await _repository!.AddAsync(todo1);
        await _repository.AddAsync(todo2);
        await _repository.AddAsync(todo3);

        var completed1 = todo1.ToggleCompleted();
        var completed2 = todo2.ToggleCompleted();
        await using var conn = await _dataSource!.OpenConnectionAsync();
        await conn.ExecuteAsync("UPDATE todos SET completed = @Completed WHERE id = @Id",
            new { completed1.Completed, Id = completed1.Id });
        await conn.ExecuteAsync("UPDATE todos SET completed = @Completed WHERE id = @Id",
            new { completed2.Completed, Id = completed2.Id });

        // Act
        var result = await _repository.GetAllAsync(completed: true, search: "Buy");

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, t => Assert.True(t.Completed));
        Assert.All(result, t => Assert.Contains("Buy", t.Title));
    }
}
