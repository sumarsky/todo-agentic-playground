import { useContext, useState } from 'react';
import { TodoContext } from '../context/TodoContextValue';
import { BulkDeleteButton } from './BulkDeleteButton';
import { InlineAddForm } from './InlineAddForm';
import { TodoItem } from './TodoItem';
import { Toolbar } from './Toolbar';

export const TodoList = () => {
  const { todos, addTodo } = useContext(TodoContext);
  const [selectedIds, setSelectedIds] = useState([]);
  const [isAdding, setIsAdding] = useState(false);

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

  const handleAddSubmit = async (title) => {
    await addTodo(title);
    setIsAdding(false);
  };

  const handleAddCancel = () => {
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
      <BulkDeleteButton selectedIds={selectedIds} />
      <ul aria-label="Todos">
        {isAdding && (
          <li>
            <InlineAddForm onSubmit={handleAddSubmit} onCancel={handleAddCancel} />
          </li>
        )}
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
