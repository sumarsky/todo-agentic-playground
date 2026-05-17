import { useContext, useState } from 'react';
import { TodoContext } from '../context/TodoContextValue';

export const TodoItem = ({ todo, selected = false, onSelectionChange }) => {
  const { updateTodo, deleteTodo } = useContext(TodoContext);
  const [isEditing, setIsEditing] = useState(false);
  const [editedTitle, setEditedTitle] = useState(todo.title);

  const handleToggle = () => {
    updateTodo(todo.id, {
      title: todo.title,
      completed: !todo.completed,
    });
  };

  const handleSave = () => {
    const trimmedTitle = editedTitle.trim();

    if (!trimmedTitle) {
      return;
    }

    updateTodo(todo.id, {
      title: trimmedTitle,
      completed: todo.completed,
    });
    setIsEditing(false);
  };

  if (isEditing) {
    return (
      <div>
        <label htmlFor={`edit-todo-${todo.id}`}>Edit todo title</label>
        <input
          id={`edit-todo-${todo.id}`}
          type="text"
          value={editedTitle}
          onChange={(event) => setEditedTitle(event.target.value)}
        />
        <button type="button" onClick={handleSave}>
          Save todo
        </button>
      </div>
    );
  }

  return (
    <div>
      {onSelectionChange ? (
        <input
          type="checkbox"
          checked={selected}
          aria-label={`Select ${todo.title}`}
          onChange={(event) => onSelectionChange(todo.id, event.target.checked)}
        />
      ) : null}
      <span
        onClick={handleToggle}
        style={todo.completed ? {
          opacity: 0.6,
          textDecoration: 'line-through',
          backgroundColor: '#f5f5f5',
        } : {}}
      >
        {todo.title}
      </span>
      <button
        type="button"
        aria-label={`Delete ${todo.title}`}
        onClick={() => deleteTodo(todo.id)}
      >
        Delete
      </button>
      <button
        type="button"
        aria-label={`Edit ${todo.title}`}
        onClick={() => setIsEditing(true)}
      >
        Edit
      </button>
    </div>
  );
};
