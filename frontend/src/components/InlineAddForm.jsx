import { useRef } from 'react';

export const InlineAddForm = ({ onSubmit, onCancel }) => {
  const inputRef = useRef(null);

  const handleSubmit = (event) => {
    event.preventDefault();
    const trimmedTitle = inputRef.current.value.trim();
    if (!trimmedTitle) return;
    onSubmit(trimmedTitle);
  };

  const handleKeyDown = (event) => {
    if (event.key === 'Enter') {
      event.preventDefault();
      const trimmedTitle = inputRef.current.value.trim();
      if (!trimmedTitle) return;
      onSubmit(trimmedTitle);
    } else if (event.key === 'Escape') {
      onCancel();
    }
  };

  return (
    <form onSubmit={handleSubmit} aria-label="Add todo">
      <label htmlFor="inline-add-title" className="sr-only">Todo title</label>
      <input
        ref={inputRef}
        id="inline-add-title"
        type="text"
        placeholder="Todo title"
        autoFocus
        onKeyDown={handleKeyDown}
      />
    </form>
  );
};
