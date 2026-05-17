import { useContext, useState } from 'react';
import { Trash2, Pencil, Check } from 'lucide-react';
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
        <button type="button" onClick={handleSave} title="Save">
          <Check size={18} />
          <span className="sr-only">Save todo</span>
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
        title="Delete"
        onClick={() => deleteTodo(todo.id)}
      >
        <Trash2 size={18} />
      </button>
      <button
        type="button"
        aria-label={`Edit ${todo.title}`}
        title="Edit"
        onClick={() => setIsEditing(true)}
      >
        <Pencil size={18} />
      </button>
    </div>
  );
};
