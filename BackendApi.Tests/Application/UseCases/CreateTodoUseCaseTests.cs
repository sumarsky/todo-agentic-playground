using BackendApi.Application.UseCases;
using BackendApi.Domain;
using BackendApi.Tests.TestDoubles;
using Xunit;

namespace BackendApi.Tests.Application.UseCases;

public class CreateTodoUseCaseTests
{
    [Fact]
    public async Task CreateTodo_WithValidTitle_ReturnsTodoWithId()
    {
        // Arrange
        var repository = new FakeTodoRepository();
        var useCase = new CreateTodoUseCase(repository);
        var title = new TodoTitle("Buy groceries");

        // Act
        var result = await useCase.Execute(title);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(default, result.Id);
        Assert.Equal(title, result.Title);
        Assert.False(result.Completed);
    }

    [Fact]
    public async Task CreateTodo_SavesToRepository()
    {
        // Arrange
        var repository = new FakeTodoRepository();
        var useCase = new CreateTodoUseCase(repository);
        var title = new TodoTitle("Test todo");

        // Act
        var created = await useCase.Execute(title);

        // Assert
        var retrieved = await repository.GetByIdAsync(created.Id);
        Assert.NotNull(retrieved);
        Assert.Equal(title, retrieved.Title);
    }
}
