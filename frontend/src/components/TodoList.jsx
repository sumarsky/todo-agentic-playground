import { useContext } from 'react';
import { TodoContext } from '../context/TodoContextValue';
import { TodoItem } from './TodoItem';

export const TodoList = () => {
  const { todos } = useContext(TodoContext);

  if (todos.length === 0) {
    return <p>No todos yet</p>;
  }

  return (
    <ul aria-label="Todos">
      {todos.map((todo) => (
        <li key={todo.id}>
          <TodoItem todo={todo} />
        </li>
      ))}
    </ul>
  );
};
