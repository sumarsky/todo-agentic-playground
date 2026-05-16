using BackendApi.Application.UseCases;
using BackendApi.Tests.TestDoubles;
using Xunit;

namespace BackendApi.Tests.Application.UseCases;

public class ToggleCompletedUseCaseTests
{
    [Fact]
    public async Task ToggleCompleted_FromFalseToTrue_UpdatesTodo()
    {
        var repository = new FakeTodoRepository();
        var createUseCase = new CreateTodoUseCase(repository);
        var useCase = new ToggleCompletedUseCase(repository);

        var todo = await createUseCase.Execute("Test todo");
        Assert.False(todo.Completed);

        var result = await useCase.Execute(todo.Id);

        Assert.NotNull(result);
        Assert.True(result.Completed);
    }

    [Fact]
    public async Task ToggleCompleted_FromTrueToFalse_UpdatesTodo()
    {
        var repository = new FakeTodoRepository();
        var createUseCase = new CreateTodoUseCase(repository);
        var useCase = new ToggleCompletedUseCase(repository);

        var todo = await createUseCase.Execute("Test todo");
        var toggled = todo.ToggleCompleted();
        await repository.UpdateAsync(toggled);
        Assert.True(toggled.Completed);

        var result = await useCase.Execute(toggled.Id);

        Assert.NotNull(result);
        Assert.False(result.Completed);
    }

    [Fact]
    public async Task ToggleCompleted_SavesChangesToRepository()
    {
        var repository = new FakeTodoRepository();
        var createUseCase = new CreateTodoUseCase(repository);
        var useCase = new ToggleCompletedUseCase(repository);

        var todo = await createUseCase.Execute("Test todo");

        await useCase.Execute(todo.Id);

        var retrieved = await repository.GetByIdAsync(todo.Id);
        Assert.NotNull(retrieved);
        Assert.True(retrieved.Completed);
    }

    [Fact]
    public async Task ToggleCompleted_TodoNotFound_ReturnsNull()
    {
        var repository = new FakeTodoRepository();
        var useCase = new ToggleCompletedUseCase(repository);

        var result = await useCase.Execute(Guid.NewGuid());

        Assert.Null(result);
    }
}
