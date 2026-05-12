using BackendApi.Application;
using BackendApi.Infrastructure;
using Xunit;

namespace BackendApi.Tests.Application;

public class TodoApplicationServiceTests
{
    [Fact]
    public void Service_CreatesAndRetrievesTodo()
    {
        // Arrange
        var repository = new InMemoryTodoRepository();
        var service = new TodoApplicationService(repository);

        // Act
        var created = service.CreateTodo("New todo");
        var all = service.ListTodos(completed: null, search: null);

        // Assert
        Assert.NotNull(created);
        Assert.Single(all);
        Assert.Equal(created.Id, all.First().Id);
    }

    [Fact]
    public void Service_UpdatesTodoTitle()
    {
        // Arrange
        var repository = new InMemoryTodoRepository();
        var service = new TodoApplicationService(repository);
        var todo = service.CreateTodo("Original");

        // Act
        var updated = service.UpdateTodoTitle(todo.Id, "Updated");

        // Assert
        Assert.NotNull(updated);
        Assert.Equal("Updated", updated.Title);
    }

    [Fact]
    public void Service_TogglesCompletedStatus()
    {
        // Arrange
        var repository = new InMemoryTodoRepository();
        var service = new TodoApplicationService(repository);
        var todo = service.CreateTodo("Test");
        Assert.False(todo.Completed);

        // Act
        var toggled = service.ToggleTodoCompleted(todo.Id);

        // Assert
        Assert.NotNull(toggled);
        Assert.True(toggled.Completed);
    }

    [Fact]
    public void Service_DeletesSingleTodo()
    {
        // Arrange
        var repository = new InMemoryTodoRepository();
        var service = new TodoApplicationService(repository);
        var todo1 = service.CreateTodo("Todo 1");
        var todo2 = service.CreateTodo("Todo 2");

        // Act
        service.DeleteTodo(todo1.Id);

        // Assert
        var all = service.ListTodos(completed: null, search: null);
        Assert.Single(all);
        Assert.Equal(todo2.Id, all.First().Id);
    }

    [Fact]
    public void Service_DeletesMultipleTodos()
    {
        // Arrange
        var repository = new InMemoryTodoRepository();
        var service = new TodoApplicationService(repository);
        var todo1 = service.CreateTodo("Todo 1");
        var todo2 = service.CreateTodo("Todo 2");
        var todo3 = service.CreateTodo("Todo 3");

        // Act
        service.DeleteMultipleTodos(new[] { todo1.Id, todo3.Id });

        // Assert
        var all = service.ListTodos(completed: null, search: null);
        Assert.Single(all);
        Assert.Equal(todo2.Id, all.First().Id);
    }
}
