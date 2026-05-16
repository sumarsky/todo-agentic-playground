using BackendApi.Application.UseCases;
using BackendApi.Infrastructure;
using Xunit;

namespace BackendApi.Tests.Application.UseCases;

public class DeleteTodoUseCaseTests
{
    [Fact]
    public async Task DeleteTodo_WithValidId_RemovesTodo()
    {
        // Arrange
        var repository = new InMemoryTodoRepository();
        var createUseCase = new CreateTodoUseCase(repository);
        var useCase = new DeleteTodoUseCase(repository);

        var todo = await createUseCase.Execute("Test todo");
        Assert.NotNull(await repository.GetByIdAsync(todo.Id));

        // Act
        await useCase.Execute(todo.Id);

        // Assert
        Assert.Null(await repository.GetByIdAsync(todo.Id));
    }

    [Fact]
    public async Task DeleteTodo_NonExistentId_DoesNotThrow()
    {
        // Arrange
        var repository = new InMemoryTodoRepository();
        var useCase = new DeleteTodoUseCase(repository);

        // Act & Assert
        await useCase.Execute(Guid.NewGuid());
    }

    [Fact]
    public async Task DeleteTodo_PreservesOtherTodos()
    {
        // Arrange
        var repository = new InMemoryTodoRepository();
        var createUseCase = new CreateTodoUseCase(repository);
        var useCase = new DeleteTodoUseCase(repository);

        var todo1 = await createUseCase.Execute("Todo 1");
        var todo2 = await createUseCase.Execute("Todo 2");
        var todo3 = await createUseCase.Execute("Todo 3");

        // Act
        await useCase.Execute(todo2.Id);

        // Assert
        Assert.NotNull(await repository.GetByIdAsync(todo1.Id));
        Assert.Null(await repository.GetByIdAsync(todo2.Id));
        Assert.NotNull(await repository.GetByIdAsync(todo3.Id));
    }
}
