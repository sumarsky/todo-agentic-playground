import { useContext, useState } from 'react';
import { TodoContext } from '../context/TodoContextValue';

export const TodoForm = () => {
  const { addTodo } = useContext(TodoContext);
  const [title, setTitle] = useState('');

  const handleSubmit = async (event) => {
    event.preventDefault();
    const trimmedTitle = title.trim();

    if (!trimmedTitle) {
      return;
    }

    await addTodo(trimmedTitle);
    setTitle('');
  };

  return (
    <form onSubmit={handleSubmit} aria-label="Add todo">
      <label htmlFor="todo-title">Todo title</label>
      <input
        id="todo-title"
        type="text"
        value={title}
        onChange={(event) => setTitle(event.target.value)}
      />
      <button type="submit">Add todo</button>
    </form>
  );
};
