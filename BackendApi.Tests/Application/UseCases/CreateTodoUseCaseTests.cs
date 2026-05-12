using BackendApi.Application.UseCases;
using BackendApi.Domain;
using BackendApi.Infrastructure;
using Xunit;

namespace BackendApi.Tests.Application.UseCases;

public class CreateTodoUseCaseTests
{
    [Fact]
    public void CreateTodo_WithValidTitle_ReturnsTodoWithId()
    {
        // Arrange
        var repository = new InMemoryTodoRepository();
        var useCase = new CreateTodoUseCase(repository);
        var title = "Buy groceries";

        // Act
        var result = useCase.Execute(title);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(title, result.Title);
        Assert.False(result.Completed);
    }

    [Fact]
    public void CreateTodo_WithEmptyTitle_ThrowsArgumentException()
    {
        // Arrange
        var repository = new InMemoryTodoRepository();
        var useCase = new CreateTodoUseCase(repository);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => useCase.Execute(""));
    }

    [Fact]
    public void CreateTodo_WithNullTitle_ThrowsArgumentException()
    {
        // Arrange
        var repository = new InMemoryTodoRepository();
        var useCase = new CreateTodoUseCase(repository);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => useCase.Execute(null!));
    }

    [Fact]
    public void CreateTodo_SavesToRepository()
    {
        // Arrange
        var repository = new InMemoryTodoRepository();
        var useCase = new CreateTodoUseCase(repository);
        var title = "Test todo";

        // Act
        var created = useCase.Execute(title);

        // Assert
        var retrieved = repository.GetById(created.Id);
        Assert.NotNull(retrieved);
        Assert.Equal(title, retrieved.Title);
    }
}
