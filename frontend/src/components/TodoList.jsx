import { useContext, useState } from 'react';
import { TodoContext } from '../context/TodoContextValue';
import { BulkDeleteButton } from './BulkDeleteButton';
import { TodoItem } from './TodoItem';
import { Toolbar } from './Toolbar';

export const TodoList = () => {
  const { todos, addTodo } = useContext(TodoContext);
  const [selectedIds, setSelectedIds] = useState([]);
  const [isAdding, setIsAdding] = useState(false);
  const [title, setTitle] = useState('');

  const handleSelectionChange = (todoId, selected) => {
    setSelectedIds((current) => {
      if (selected) {
        return current.includes(todoId) ? current : [...current, todoId];
      }
      return current.filter((id) => id !== todoId);
    });
  };

  const handleSelectAll = () => {
    if (selectedIds.length === todos.length) {
      setSelectedIds([]);
    } else {
      setSelectedIds(todos.map((todo) => todo.id));
    }
  };

  const handleAddClick = () => {
    setIsAdding(true);
  };

  const handleAddSubmit = async (event) => {
    event.preventDefault();
    const trimmedTitle = title.trim();
    if (!trimmedTitle) return;
    await addTodo(trimmedTitle);
    setTitle('');
    setIsAdding(false);
  };

  const isAllSelected = todos.length > 0 && selectedIds.length === todos.length;

  if (todos.length === 0 && !isAdding) {
    return (
      <>
        <Toolbar onAddClick={handleAddClick} onSelectAll={handleSelectAll} isAllSelected={isAllSelected} />
        <p>No todos yet</p>
      </>
    );
  }

  return (
    <>
      <Toolbar onAddClick={handleAddClick} onSelectAll={handleSelectAll} isAllSelected={isAllSelected} />
      {isAdding && (
        <form onSubmit={handleAddSubmit} aria-label="Add todo">
          <label htmlFor="todo-title">Todo title</label>
          <input
            id="todo-title"
            type="text"
            value={title}
            onChange={(event) => setTitle(event.target.value)}
            autoFocus
          />
          <button type="submit">Add todo</button>
        </form>
      )}
      <BulkDeleteButton selectedIds={selectedIds} />
      <ul aria-label="Todos">
        {todos.map((todo) => (
          <li key={todo.id}>
            <TodoItem
              todo={todo}
              selected={selectedIds.includes(todo.id)}
              onSelectionChange={handleSelectionChange}
            />
          </li>
        ))}
      </ul>
    </>
  );
};
