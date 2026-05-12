namespace BackendApi.Domain;

public interface ITodoRepository
{
    void Add(Todo todo);
    Todo? GetById(Guid id);
    IReadOnlyList<Todo> GetAll();
    void Update(Todo todo);
    void Delete(Guid id);
    void DeleteByIds(IEnumerable<Guid> ids);
}
