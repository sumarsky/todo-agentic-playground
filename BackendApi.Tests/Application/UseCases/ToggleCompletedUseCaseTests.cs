using BackendApi.Application.UseCases;
using BackendApi.Infrastructure;
using Xunit;

namespace BackendApi.Tests.Application.UseCases;

public class ToggleCompletedUseCaseTests
{
    [Fact]
    public void ToggleCompleted_FromFalseToTrue_UpdatesTodo()
    {
        var repository = new InMemoryTodoRepository();
        var createUseCase = new CreateTodoUseCase(repository);
        var useCase = new ToggleCompletedUseCase(repository);

        var todo = createUseCase.Execute("Test todo");
        Assert.False(todo.Completed);

        var result = useCase.Execute(todo.Id);

        Assert.NotNull(result);
        Assert.True(result.Completed);
    }

    [Fact]
    public void ToggleCompleted_FromTrueToFalse_UpdatesTodo()
    {
        var repository = new InMemoryTodoRepository();
        var createUseCase = new CreateTodoUseCase(repository);
        var useCase = new ToggleCompletedUseCase(repository);

        var todo = createUseCase.Execute("Test todo");
        var toggled = todo.ToggleCompleted();
        repository.Update(toggled);
        Assert.True(toggled.Completed);

        var result = useCase.Execute(toggled.Id);

        Assert.NotNull(result);
        Assert.False(result.Completed);
    }

    [Fact]
    public void ToggleCompleted_SavesChangesToRepository()
    {
        var repository = new InMemoryTodoRepository();
        var createUseCase = new CreateTodoUseCase(repository);
        var useCase = new ToggleCompletedUseCase(repository);

        var todo = createUseCase.Execute("Test todo");

        useCase.Execute(todo.Id);

        var retrieved = repository.GetById(todo.Id);
        Assert.NotNull(retrieved);
        Assert.True(retrieved.Completed);
    }

    [Fact]
    public void ToggleCompleted_TodoNotFound_ReturnsNull()
    {
        var repository = new InMemoryTodoRepository();
        var useCase = new ToggleCompletedUseCase(repository);

        var result = useCase.Execute(Guid.NewGuid());

        Assert.Null(result);
    }
}
