import { useContext } from 'react';
import { TodoContext } from '../context/TodoContextValue';

export const BulkDeleteButton = ({ selectedIds }) => {
  const { bulkDeleteTodos } = useContext(TodoContext);
  const count = selectedIds.length;

  return (
    <button
      type="button"
      disabled={count === 0}
      onClick={() => bulkDeleteTodos(selectedIds)}
    >
      Delete selected ({count})
    </button>
  );
};
