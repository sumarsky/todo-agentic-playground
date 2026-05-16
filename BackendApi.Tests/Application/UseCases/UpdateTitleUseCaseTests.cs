using BackendApi.Application.UseCases;
using BackendApi.Domain;
using BackendApi.Infrastructure;
using Xunit;

namespace BackendApi.Tests.Application.UseCases;

public class UpdateTitleUseCaseTests
{
    [Fact]
    public async Task UpdateTitle_WithValidTitle_UpdatesTodo()
    {
        // Arrange
        var repository = new InMemoryTodoRepository();
        var createUseCase = new CreateTodoUseCase(repository);
        var useCase = new UpdateTitleUseCase(repository);

        var todo = await createUseCase.Execute("Old title");
        var newTitle = "New title";

        // Act
        var result = await useCase.Execute(todo.Id, newTitle);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(todo.Id, result.Id);
        Assert.Equal(newTitle, result.Title);
    }

    [Fact]
    public async Task UpdateTitle_SavesChangesToRepository()
    {
        // Arrange
        var repository = new InMemoryTodoRepository();
        var createUseCase = new CreateTodoUseCase(repository);
        var useCase = new UpdateTitleUseCase(repository);

        var todo = await createUseCase.Execute("Old title");

        // Act
        await useCase.Execute(todo.Id, "New title");

        // Assert
        var retrieved = await repository.GetByIdAsync(todo.Id);
        Assert.NotNull(retrieved);
        Assert.Equal("New title", retrieved.Title);
    }

    [Fact]
    public async Task UpdateTitle_WithEmptyTitle_ThrowsArgumentException()
    {
        // Arrange
        var repository = new InMemoryTodoRepository();
        var useCase = new UpdateTitleUseCase(repository);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => useCase.Execute(Guid.NewGuid(), ""));
    }

    [Fact]
    public async Task UpdateTitle_TodoNotFound_ReturnsNull()
    {
        // Arrange
        var repository = new InMemoryTodoRepository();
        var useCase = new UpdateTitleUseCase(repository);

        // Act
        var result = await useCase.Execute(Guid.NewGuid(), "New title");

        // Assert
        Assert.Null(result);
    }
}
