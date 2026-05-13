import { useContext, useState } from 'react';
import { TodoContext } from '../context/TodoContextValue';

export const TodoItem = ({ todo }) => {
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
      <label>
        <input
          type="checkbox"
          checked={todo.completed}
          aria-label={`Mark ${todo.title} complete`}
          onChange={handleToggle}
        />
        <span>{todo.title}</span>
      </label>
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
