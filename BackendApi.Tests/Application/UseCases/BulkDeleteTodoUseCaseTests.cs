using BackendApi.Application.UseCases;
using BackendApi.Infrastructure;
using Xunit;

namespace BackendApi.Tests.Application.UseCases;

public class BulkDeleteTodoUseCaseTests
{
    [Fact]
    public void BulkDeleteTodos_WithMultipleIds_RemovesAllTodos()
    {
        // Arrange
        var repository = new InMemoryTodoRepository();
        var createUseCase = new CreateTodoUseCase(repository);
        var useCase = new BulkDeleteTodoUseCase(repository);

        var todo1 = createUseCase.Execute("Todo 1");
        var todo2 = createUseCase.Execute("Todo 2");
        var todo3 = createUseCase.Execute("Todo 3");

        var idsToDelete = new[] { todo1.Id, todo2.Id };

        // Act
        useCase.Execute(idsToDelete);

        // Assert
        Assert.Null(repository.GetById(todo1.Id));
        Assert.Null(repository.GetById(todo2.Id));
        Assert.NotNull(repository.GetById(todo3.Id));
    }

    [Fact]
    public void BulkDeleteTodos_WithEmptyList_DoesNotThrow()
    {
        // Arrange
        var repository = new InMemoryTodoRepository();
        var useCase = new BulkDeleteTodoUseCase(repository);

        // Act & Assert
        useCase.Execute(Array.Empty<Guid>());
    }

    [Fact]
    public void BulkDeleteTodos_PreservesUnaffectedTodos()
    {
        // Arrange
        var repository = new InMemoryTodoRepository();
        var createUseCase = new CreateTodoUseCase(repository);
        var useCase = new BulkDeleteTodoUseCase(repository);

        var todo1 = createUseCase.Execute("Todo 1");
        var todo2 = createUseCase.Execute("Todo 2");
        var todo3 = createUseCase.Execute("Todo 3");
        var todo4 = createUseCase.Execute("Todo 4");

        var idsToDelete = new[] { todo1.Id, todo3.Id };

        // Act
        useCase.Execute(idsToDelete);

        // Assert
        Assert.Null(repository.GetById(todo1.Id));
        Assert.NotNull(repository.GetById(todo2.Id));
        Assert.Null(repository.GetById(todo3.Id));
        Assert.NotNull(repository.GetById(todo4.Id));
    }
}
