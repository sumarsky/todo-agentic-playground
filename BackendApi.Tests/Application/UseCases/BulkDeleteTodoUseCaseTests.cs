using BackendApi.Application.UseCases;
using BackendApi.Infrastructure;
using Xunit;

namespace BackendApi.Tests.Application.UseCases;

public class BulkDeleteTodoUseCaseTests
{
    [Fact]
    public async Task BulkDeleteTodos_WithMultipleIds_RemovesAllTodos()
    {
        // Arrange
        var repository = new InMemoryTodoRepository();
        var createUseCase = new CreateTodoUseCase(repository);
        var useCase = new BulkDeleteTodoUseCase(repository);

        var todo1 = await createUseCase.Execute("Todo 1");
        var todo2 = await createUseCase.Execute("Todo 2");
        var todo3 = await createUseCase.Execute("Todo 3");

        var idsToDelete = new[] { todo1.Id, todo2.Id };

        // Act
        await useCase.Execute(idsToDelete);

        // Assert
        Assert.Null(await repository.GetByIdAsync(todo1.Id));
        Assert.Null(await repository.GetByIdAsync(todo2.Id));
        Assert.NotNull(await repository.GetByIdAsync(todo3.Id));
    }

    [Fact]
    public async Task BulkDeleteTodos_WithEmptyList_DoesNotThrow()
    {
        // Arrange
        var repository = new InMemoryTodoRepository();
        var useCase = new BulkDeleteTodoUseCase(repository);

        // Act & Assert
        await useCase.Execute(Array.Empty<Guid>());
    }

    [Fact]
    public async Task BulkDeleteTodos_PreservesUnaffectedTodos()
    {
        // Arrange
        var repository = new InMemoryTodoRepository();
        var createUseCase = new CreateTodoUseCase(repository);
        var useCase = new BulkDeleteTodoUseCase(repository);

        var todo1 = await createUseCase.Execute("Todo 1");
        var todo2 = await createUseCase.Execute("Todo 2");
        var todo3 = await createUseCase.Execute("Todo 3");
        var todo4 = await createUseCase.Execute("Todo 4");

        var idsToDelete = new[] { todo1.Id, todo3.Id };

        // Act
        await useCase.Execute(idsToDelete);

        // Assert
        Assert.Null(await repository.GetByIdAsync(todo1.Id));
        Assert.NotNull(await repository.GetByIdAsync(todo2.Id));
        Assert.Null(await repository.GetByIdAsync(todo3.Id));
        Assert.NotNull(await repository.GetByIdAsync(todo4.Id));
    }
}
