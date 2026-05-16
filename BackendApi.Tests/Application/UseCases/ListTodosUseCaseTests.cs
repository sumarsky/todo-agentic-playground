using BackendApi.Application.UseCases;
using BackendApi.Domain;
using BackendApi.Tests.TestDoubles;
using Xunit;

namespace BackendApi.Tests.Application.UseCases;

public class ListTodosUseCaseTests
{
    private readonly FakeTodoRepository _repository;
    private readonly CreateTodoUseCase _createUseCase;
    private readonly ListTodosUseCase _useCase;

    public ListTodosUseCaseTests()
    {
        _repository = new FakeTodoRepository();
        _createUseCase = new CreateTodoUseCase(_repository);
        _useCase = new ListTodosUseCase(_repository);
    }

    [Fact]
    public async Task ListTodos_NoFilter_ReturnsAllTodos()
    {
        // Arrange
        await _createUseCase.Execute("Buy milk");
        await _createUseCase.Execute("Walk dog");
        await _createUseCase.Execute("Code review");

        // Act
        var result = await _useCase.Execute(completed: null, search: null);

        // Assert
        Assert.Equal(3, result.Count);
    }

    [Fact]
    public async Task ListTodos_FilterCompleted_ReturnsOnlyCompletedTodos()
    {
        // Arrange
        var todo1 = await _createUseCase.Execute("Task 1");
        var todo2 = await _createUseCase.Execute("Task 2");
        var todo3 = await _createUseCase.Execute("Task 3");
        
        var toggled1 = todo1.ToggleCompleted();
        await _repository.UpdateAsync(toggled1);
        var toggled3 = todo3.ToggleCompleted();
        await _repository.UpdateAsync(toggled3);

        // Act
        var result = await _useCase.Execute(completed: true, search: null);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, t => Assert.True(t.Completed));
    }

    [Fact]
    public async Task ListTodos_FilterNotCompleted_ReturnsOnlyIncompleteTodos()
    {
        // Arrange
        var todo1 = await _createUseCase.Execute("Task 1");
        var todo2 = await _createUseCase.Execute("Task 2");
        var todo3 = await _createUseCase.Execute("Task 3");
        
        var toggled1 = todo1.ToggleCompleted();
        await _repository.UpdateAsync(toggled1);

        // Act
        var result = await _useCase.Execute(completed: false, search: null);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, t => Assert.False(t.Completed));
    }

    [Fact]
    public async Task ListTodos_SearchByTitle_ReturnsMatchingTodos()
    {
        // Arrange
        await _createUseCase.Execute("Buy groceries");
        await _createUseCase.Execute("Buy milk");
        await _createUseCase.Execute("Walk dog");

        // Act
        var result = await _useCase.Execute(completed: null, search: "Buy");

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, t => Assert.Contains("Buy", t.Title));
    }

    [Fact]
    public async Task ListTodos_SearchCaseInsensitive()
    {
        // Arrange
        await _createUseCase.Execute("Buy Groceries");
        await _createUseCase.Execute("Walk dog");

        // Act
        var result = await _useCase.Execute(completed: null, search: "buy");

        // Assert
        Assert.Single(result);
        Assert.Equal("Buy Groceries", result.First().Title);
    }

    [Fact]
    public async Task ListTodos_CombineFiltersAndSearch()
    {
        // Arrange
        var todo1 = await _createUseCase.Execute("Buy milk");
        var todo2 = await _createUseCase.Execute("Buy groceries");
        var todo3 = await _createUseCase.Execute("Walk dog");
        
        var toggled1 = todo1.ToggleCompleted();
        await _repository.UpdateAsync(toggled1);
        var toggled2 = todo2.ToggleCompleted();
        await _repository.UpdateAsync(toggled2);

        // Act
        var result = await _useCase.Execute(completed: true, search: "Buy");

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, t => Assert.True(t.Completed));
        Assert.All(result, t => Assert.Contains("Buy", t.Title));
    }
}
