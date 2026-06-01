using BackendApi.Application.UseCases;
using BackendApi.Domain;
using BackendApi.Tests.TestDoubles;
using Xunit;

namespace BackendApi.Tests.Application.UseCases;

public class UpdateTitleUseCaseTests
{
    [Fact]
    public async Task UpdateTitle_WithValidTitle_UpdatesTodo()
    {
        // Arrange
        var repository = new FakeTodoRepository();
        var createUseCase = new CreateTodoUseCase(repository);
        var useCase = new UpdateTitleUseCase(repository);

        var todo = await createUseCase.Execute(new TodoTitle("Old title"));
        var newTitle = new TodoTitle("New title");

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
        var repository = new FakeTodoRepository();
        var createUseCase = new CreateTodoUseCase(repository);
        var useCase = new UpdateTitleUseCase(repository);

        var todo = await createUseCase.Execute(new TodoTitle("Old title"));

        // Act
        var newTitle = new TodoTitle("New title");
        await useCase.Execute(todo.Id, newTitle);

        // Assert
        var retrieved = await repository.GetByIdAsync(todo.Id);
        Assert.NotNull(retrieved);
        Assert.Equal(newTitle, retrieved.Title);
    }

    [Fact]
    public async Task UpdateTitle_TodoNotFound_ReturnsNull()
    {
        // Arrange
        var repository = new FakeTodoRepository();
        var useCase = new UpdateTitleUseCase(repository);

        // Act
        var result = await useCase.Execute(TodoId.New(), new TodoTitle("New title"));

        // Assert
        Assert.Null(result);
    }
}
