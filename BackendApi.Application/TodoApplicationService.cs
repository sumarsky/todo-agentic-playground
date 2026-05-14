using BackendApi.Application.Ports;
using BackendApi.Application.UseCases;
using BackendApi.Domain;

namespace BackendApi.Application;

public class TodoApplicationService
{
    private readonly CreateTodoUseCase _createTodoUseCase;
    private readonly ListTodosUseCase _listTodosUseCase;
    private readonly UpdateTitleUseCase _updateTitleUseCase;
    private readonly ToggleCompletedUseCase _toggleCompletedUseCase;
    private readonly DeleteTodoUseCase _deleteTodoUseCase;
    private readonly BulkDeleteTodoUseCase _bulkDeleteTodoUseCase;

    public TodoApplicationService(ITodoRepository repository)
    {
        _createTodoUseCase = new CreateTodoUseCase(repository);
        _listTodosUseCase = new ListTodosUseCase(repository);
        _updateTitleUseCase = new UpdateTitleUseCase(repository);
        _toggleCompletedUseCase = new ToggleCompletedUseCase(repository);
        _deleteTodoUseCase = new DeleteTodoUseCase(repository);
        _bulkDeleteTodoUseCase = new BulkDeleteTodoUseCase(repository);
    }

    public Todo CreateTodo(string title)
    {
        return _createTodoUseCase.Execute(title);
    }

    public IReadOnlyList<Todo> ListTodos(bool? completed, string? search)
    {
        return _listTodosUseCase.Execute(completed, search);
    }

    public Todo? UpdateTodoTitle(Guid id, string newTitle)
    {
        return _updateTitleUseCase.Execute(id, newTitle);
    }

    public Todo? ToggleTodoCompleted(Guid id)
    {
        return _toggleCompletedUseCase.Execute(id);
    }

    public void DeleteTodo(Guid id)
    {
        _deleteTodoUseCase.Execute(id);
    }

    public void DeleteMultipleTodos(IEnumerable<Guid> ids)
    {
        _bulkDeleteTodoUseCase.Execute(ids);
    }
}
