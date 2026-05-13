import { describe, it, expect } from 'vitest';
import { TodoContext } from './TodoContext';

describe('TodoContext', () => {
  it('has default state: todos=[], loading=false, error=null', () => {
    expect(TodoContext).toBeDefined();
    expect(TodoContext._currentValue).toEqual({
      todos: [],
      loading: false,
      error: null,
    });
  });
});
