import { describe, it, expect } from 'vitest';
import { render, screen } from '@testing-library/react';
import { useContext } from 'react';
import { TodoContextProvider } from './TodoContext';
import { TodoContext } from './TodoContextValue';

const TestConsumer = () => {
  const ctx = useContext(TodoContext);
  return (
    <div>
      <span data-testid="todos-count">{ctx.todos.length}</span>
      <span data-testid="loading">{String(ctx.loading)}</span>
      <span data-testid="error">{ctx.error || 'none'}</span>
    </div>
  );
};

describe('TodoContextProvider', () => {
  it('provides default context state to children', () => {
    render(
      <TodoContextProvider>
        <TestConsumer />
      </TodoContextProvider>
    );
    
    expect(screen.getByTestId('todos-count')).toHaveTextContent('0');
    expect(screen.getByTestId('loading')).toHaveTextContent('false');
    expect(screen.getByTestId('error')).toHaveTextContent('none');
  });
});
