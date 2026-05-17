import { useContext, useState } from 'react';
import { TodoContext } from '../context/TodoContextValue';
import { BulkDeleteButton } from './BulkDeleteButton';
import { InlineAddForm } from './InlineAddForm';
import { TodoItem } from './TodoItem';
import { Toolbar } from './Toolbar';

export const TodoList = () => {
  const { todos, addTodo, selectedIds, selectAll, deselectAll, selectTodo, deselectTodo } = useContext(TodoContext);
  const [isAdding, setIsAdding] = useState(false);

  const handleSelectionChange = (todoId, selected) => {
    if (selected) {
      selectTodo(todoId);
    } else {
      deselectTodo(todoId);
    }
  };

  const handleSelectAll = () => {
    if (selectedIds.length === todos.length) {
      deselectAll();
    } else {
      selectAll();
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
  const isIndeterminate = selectedIds.length > 0 && selectedIds.length < todos.length;

  if (todos.length === 0 && !isAdding) {
    return (
      <>
        <Toolbar onAddClick={handleAddClick} onSelectAll={handleSelectAll} isAllSelected={isAllSelected} isIndeterminate={isIndeterminate} />
        <p>No todos yet</p>
      </>
    );
  }

  return (
    <>
      <Toolbar onAddClick={handleAddClick} onSelectAll={handleSelectAll} isAllSelected={isAllSelected} isIndeterminate={isIndeterminate} />
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
