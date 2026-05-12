using BackendApi.Application.UseCases;
using BackendApi.Infrastructure;
using Xunit;

namespace BackendApi.Tests.Application.UseCases;

public class ToggleCompletedUseCaseTests
{
    [Fact]
    public void ToggleCompleted_FromFalseToTrue_UpdatesTodo()
    {
        // Arrange
        var repository = new InMemoryTodoRepository();
        var createUseCase = new CreateTodoUseCase(repository);
        var useCase = new ToggleCompletedUseCase(repository);

        var todo = createUseCase.Execute("Test todo");
        Assert.False(todo.Completed);

        // Act
        var result = useCase.Execute(todo.Id);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Completed);
    }

    [Fact]
    public void ToggleCompleted_FromTrueToFalse_UpdatesTodo()
    {
        // Arrange
        var repository = new InMemoryTodoRepository();
        var createUseCase = new CreateTodoUseCase(repository);
        var useCase = new ToggleCompletedUseCase(repository);

        var todo = createUseCase.Execute("Test todo");
        todo.ToggleCompleted();
        repository.Update(todo);
        Assert.True(todo.Completed);

        // Act
        var result = useCase.Execute(todo.Id);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Completed);
    }

    [Fact]
    public void ToggleCompleted_SavesChangesToRepository()
    {
        // Arrange
        var repository = new InMemoryTodoRepository();
        var createUseCase = new CreateTodoUseCase(repository);
        var useCase = new ToggleCompletedUseCase(repository);

        var todo = createUseCase.Execute("Test todo");

        // Act
        useCase.Execute(todo.Id);

        // Assert
        var retrieved = repository.GetById(todo.Id);
        Assert.NotNull(retrieved);
        Assert.True(retrieved.Completed);
    }

    [Fact]
    public void ToggleCompleted_TodoNotFound_ReturnsNull()
    {
        // Arrange
        var repository = new InMemoryTodoRepository();
        var useCase = new ToggleCompletedUseCase(repository);

        // Act
        var result = useCase.Execute(Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }
}
