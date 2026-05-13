import { useContext, useState } from 'react';
import { TodoContext } from '../context/TodoContextValue';
import { BulkDeleteButton } from './BulkDeleteButton';
import { TodoItem } from './TodoItem';

export const TodoList = () => {
  const { todos } = useContext(TodoContext);
  const [selectedIds, setSelectedIds] = useState([]);

  const handleSelectionChange = (todoId, selected) => {
    setSelectedIds((current) => {
      if (selected) {
        return current.includes(todoId) ? current : [...current, todoId];
      }
      return current.filter((id) => id !== todoId);
    });
  };

  if (todos.length === 0) {
    return <p>No todos yet</p>;
  }

  return (
    <>
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
