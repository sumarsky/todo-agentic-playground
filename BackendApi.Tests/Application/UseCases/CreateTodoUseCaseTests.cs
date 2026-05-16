using BackendApi.Application.UseCases;
using BackendApi.Domain;
using BackendApi.Infrastructure;
using Xunit;

namespace BackendApi.Tests.Application.UseCases;

public class CreateTodoUseCaseTests
{
    [Fact]
    public async Task CreateTodo_WithValidTitle_ReturnsTodoWithId()
    {
        // Arrange
        var repository = new InMemoryTodoRepository();
        var useCase = new CreateTodoUseCase(repository);
        var title = "Buy groceries";

        // Act
        var result = await useCase.Execute(title);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(title, result.Title);
        Assert.False(result.Completed);
    }

    [Fact]
    public async Task CreateTodo_WithEmptyTitle_ThrowsArgumentException()
    {
        // Arrange
        var repository = new InMemoryTodoRepository();
        var useCase = new CreateTodoUseCase(repository);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => useCase.Execute(""));
    }

    [Fact]
    public async Task CreateTodo_WithNullTitle_ThrowsArgumentException()
    {
        // Arrange
        var repository = new InMemoryTodoRepository();
        var useCase = new CreateTodoUseCase(repository);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => useCase.Execute(null!));
    }

    [Fact]
    public async Task CreateTodo_SavesToRepository()
    {
        // Arrange
        var repository = new InMemoryTodoRepository();
        var useCase = new CreateTodoUseCase(repository);
        var title = "Test todo";

        // Act
        var created = await useCase.Execute(title);

        // Assert
        var retrieved = await repository.GetByIdAsync(created.Id);
        Assert.NotNull(retrieved);
        Assert.Equal(title, retrieved.Title);
    }
}
