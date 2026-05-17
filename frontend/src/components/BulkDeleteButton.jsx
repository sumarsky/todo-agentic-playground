import { useContext } from 'react';
import { Trash2 } from 'lucide-react';
import { TodoContext } from '../context/TodoContextValue';

export const BulkDeleteButton = ({ selectedIds }) => {
  const { bulkDeleteTodos } = useContext(TodoContext);
  const count = selectedIds.length;

  return (
    <button
      type="button"
      disabled={count === 0}
      title="Delete selected"
      onClick={() => bulkDeleteTodos(selectedIds)}
    >
      <Trash2 size={18} />
      <span className="sr-only">Delete selected ({count})</span>
    </button>
  );
};
