using BackendApi.Application.UseCases;
using BackendApi.Domain;
using BackendApi.Infrastructure;
using Xunit;

namespace BackendApi.Tests.Application.UseCases;

public class ListTodosUseCaseTests
{
    private readonly InMemoryTodoRepository _repository;
    private readonly CreateTodoUseCase _createUseCase;
    private readonly ListTodosUseCase _useCase;

    public ListTodosUseCaseTests()
    {
        _repository = new InMemoryTodoRepository();
        _createUseCase = new CreateTodoUseCase(_repository);
        _useCase = new ListTodosUseCase(_repository);
    }

    [Fact]
    public void ListTodos_NoFilter_ReturnsAllTodos()
    {
        // Arrange
        _createUseCase.Execute("Buy milk");
        _createUseCase.Execute("Walk dog");
        _createUseCase.Execute("Code review");

        // Act
        var result = _useCase.Execute(completed: null, search: null);

        // Assert
        Assert.Equal(3, result.Count);
    }

    [Fact]
    public void ListTodos_FilterCompleted_ReturnsOnlyCompletedTodos()
    {
        // Arrange
        var todo1 = _createUseCase.Execute("Task 1");
        var todo2 = _createUseCase.Execute("Task 2");
        var todo3 = _createUseCase.Execute("Task 3");
        
        todo1.ToggleCompleted();
        _repository.Update(todo1);
        todo3.ToggleCompleted();
        _repository.Update(todo3);

        // Act
        var result = _useCase.Execute(completed: true, search: null);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, t => Assert.True(t.Completed));
    }

    [Fact]
    public void ListTodos_FilterNotCompleted_ReturnsOnlyIncompleteTodos()
    {
        // Arrange
        var todo1 = _createUseCase.Execute("Task 1");
        var todo2 = _createUseCase.Execute("Task 2");
        var todo3 = _createUseCase.Execute("Task 3");
        
        todo1.ToggleCompleted();
        _repository.Update(todo1);

        // Act
        var result = _useCase.Execute(completed: false, search: null);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, t => Assert.False(t.Completed));
    }

    [Fact]
    public void ListTodos_SearchByTitle_ReturnsMatchingTodos()
    {
        // Arrange
        _createUseCase.Execute("Buy groceries");
        _createUseCase.Execute("Buy milk");
        _createUseCase.Execute("Walk dog");

        // Act
        var result = _useCase.Execute(completed: null, search: "Buy");

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, t => Assert.Contains("Buy", t.Title));
    }

    [Fact]
    public void ListTodos_SearchCaseInsensitive()
    {
        // Arrange
        _createUseCase.Execute("Buy Groceries");
        _createUseCase.Execute("Walk dog");

        // Act
        var result = _useCase.Execute(completed: null, search: "buy");

        // Assert
        Assert.Single(result);
        Assert.Equal("Buy Groceries", result.First().Title);
    }

    [Fact]
    public void ListTodos_CombineFiltersAndSearch()
    {
        // Arrange
        var todo1 = _createUseCase.Execute("Buy milk");
        var todo2 = _createUseCase.Execute("Buy groceries");
        var todo3 = _createUseCase.Execute("Walk dog");
        
        todo1.ToggleCompleted();
        _repository.Update(todo1);
        todo2.ToggleCompleted();
        _repository.Update(todo2);

        // Act
        var result = _useCase.Execute(completed: true, search: "Buy");

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, t => Assert.True(t.Completed));
        Assert.All(result, t => Assert.Contains("Buy", t.Title));
    }
}
