using BackendApi.Application.UseCases;
using BackendApi.Infrastructure;
using Xunit;

namespace BackendApi.Tests.Application.UseCases;

public class DeleteTodoUseCaseTests
{
    [Fact]
    public void DeleteTodo_WithValidId_RemovesTodo()
    {
        // Arrange
        var repository = new InMemoryTodoRepository();
        var createUseCase = new CreateTodoUseCase(repository);
        var useCase = new DeleteTodoUseCase(repository);

        var todo = createUseCase.Execute("Test todo");
        Assert.NotNull(repository.GetById(todo.Id));

        // Act
        useCase.Execute(todo.Id);

        // Assert
        Assert.Null(repository.GetById(todo.Id));
    }

    [Fact]
    public void DeleteTodo_NonExistentId_DoesNotThrow()
    {
        // Arrange
        var repository = new InMemoryTodoRepository();
        var useCase = new DeleteTodoUseCase(repository);

        // Act & Assert
        useCase.Execute(Guid.NewGuid());
    }

    [Fact]
    public void DeleteTodo_PreservesOtherTodos()
    {
        // Arrange
        var repository = new InMemoryTodoRepository();
        var createUseCase = new CreateTodoUseCase(repository);
        var useCase = new DeleteTodoUseCase(repository);

        var todo1 = createUseCase.Execute("Todo 1");
        var todo2 = createUseCase.Execute("Todo 2");
        var todo3 = createUseCase.Execute("Todo 3");

        // Act
        useCase.Execute(todo2.Id);

        // Assert
        Assert.NotNull(repository.GetById(todo1.Id));
        Assert.Null(repository.GetById(todo2.Id));
        Assert.NotNull(repository.GetById(todo3.Id));
    }
}
