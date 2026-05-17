import { describe, it, expect, vi } from 'vitest';
import { fireEvent, render, screen, waitFor } from '@testing-library/react';
import { TodoContext } from '../context/TodoContextValue';
import { BulkDeleteButton } from './BulkDeleteButton';
import { FilterBar } from './FilterBar';
import { TodoForm } from './TodoForm';
import { TodoItem } from './TodoItem';
import { TodoList } from './TodoList';
import { Toolbar } from './Toolbar';

const renderWithTodos = (ui, contextOverrides = {}) => {
  const value = {
    todos: [],
    loading: false,
    error: null,
    listTodos: vi.fn(),
    addTodo: vi.fn(),
    updateTodo: vi.fn(),
    deleteTodo: vi.fn(),
    bulkDeleteTodos: vi.fn(),
    filters: { completed: false, search: '' },
    setCompletedFilter: vi.fn(),
    setSearchFilter: vi.fn(),
    ...contextOverrides,
  };

  return {
    ...render(
      <TodoContext.Provider value={value}>
        {ui}
      </TodoContext.Provider>
    ),
    value,
  };
};

describe('Toolbar', () => {
  it('renders search input, add button, filter toggle, and select-all checkbox', () => {
    const onAddClick = vi.fn();
    const onSelectAll = vi.fn();

    renderWithTodos(
      <Toolbar onAddClick={onAddClick} onSelectAll={onSelectAll} isAllSelected={false} />
    );

    expect(screen.getByRole('textbox', { name: /search todos/i })).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /add todo/i })).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /filter completed/i })).toBeInTheDocument();
    expect(screen.getByRole('checkbox', { name: /select all/i })).toBeInTheDocument();
  });

  it('calls setSearchFilter and listTodos when search input changes', () => {
    const setSearchFilter = vi.fn();
    const listTodos = vi.fn();

    renderWithTodos(
      <Toolbar onAddClick={vi.fn()} onSelectAll={vi.fn()} isAllSelected={false} />,
      {
        filters: { completed: false, search: '' },
        setSearchFilter,
        listTodos,
      }
    );

    fireEvent.change(screen.getByRole('textbox', { name: /search todos/i }), {
      target: { value: 'docs' },
    });

    expect(setSearchFilter).toHaveBeenCalledWith('docs');
    expect(listTodos).toHaveBeenCalledWith({ completed: false, search: 'docs' });
  });

  it('toggles completed filter and refetches todos', () => {
    const setCompletedFilter = vi.fn();
    const listTodos = vi.fn();

    renderWithTodos(
      <Toolbar onAddClick={vi.fn()} onSelectAll={vi.fn()} isAllSelected={false} />,
      {
        filters: { completed: false, search: '' },
        setCompletedFilter,
        listTodos,
      }
    );

    fireEvent.click(screen.getByRole('button', { name: /filter completed/i }));

    expect(setCompletedFilter).toHaveBeenCalledWith(true);
    expect(listTodos).toHaveBeenCalledWith({ completed: true, search: '' });
  });

  it('calls onAddClick when add button is clicked', () => {
    const onAddClick = vi.fn();

    renderWithTodos(
      <Toolbar onAddClick={onAddClick} onSelectAll={vi.fn()} isAllSelected={false} />
    );

    fireEvent.click(screen.getByRole('button', { name: /add todo/i }));

    expect(onAddClick).toHaveBeenCalledTimes(1);
  });

  it('calls onSelectAll when select-all checkbox is clicked', () => {
    const onSelectAll = vi.fn();

    renderWithTodos(
      <Toolbar onAddClick={vi.fn()} onSelectAll={onSelectAll} isAllSelected={false} />
    );

    fireEvent.click(screen.getByRole('checkbox', { name: /select all/i }));

    expect(onSelectAll).toHaveBeenCalledTimes(1);
  });
});

describe('FilterBar', () => {
  it('updates completed filter and refetches todos', () => {
    const setCompletedFilter = vi.fn();
    const listTodos = vi.fn();

    renderWithTodos(<FilterBar />, {
      filters: { completed: false, search: '' },
      setCompletedFilter,
      listTodos,
    });

    fireEvent.click(screen.getByRole('button', { name: /show completed todos/i }));

    expect(setCompletedFilter).toHaveBeenCalledWith(true);
    expect(listTodos).toHaveBeenCalledWith({ completed: true, search: '' });
  });

  it('updates search filter and refetches todos', () => {
    const setSearchFilter = vi.fn();
    const listTodos = vi.fn();

    renderWithTodos(<FilterBar />, {
      filters: { completed: true, search: '' },
      setSearchFilter,
      listTodos,
    });

    fireEvent.change(screen.getByLabelText(/search todos/i), {
      target: { value: 'docs' },
    });

    expect(setSearchFilter).toHaveBeenCalledWith('docs');
    expect(listTodos).toHaveBeenCalledWith({ completed: true, search: 'docs' });
  });
});

describe('BulkDeleteButton', () => {
  it('shows selected count and bulk deletes selected todo IDs', () => {
    const bulkDeleteTodos = vi.fn();

    renderWithTodos(<BulkDeleteButton selectedIds={['1', '3']} />, {
      bulkDeleteTodos,
    });

    fireEvent.click(screen.getByRole('button', { name: /delete selected \(2\)/i }));

    expect(bulkDeleteTodos).toHaveBeenCalledWith(['1', '3']);
  });
});

describe('TodoForm', () => {
  it('adds a todo with the entered title and clears the input', async () => {
    const addTodo = vi.fn().mockResolvedValue([]);

    renderWithTodos(<TodoForm />, { addTodo });

    const input = screen.getByLabelText(/todo title/i);
    fireEvent.change(input, { target: { value: 'Write component tests' } });
    fireEvent.click(screen.getByRole('button', { name: /add todo/i }));

    expect(addTodo).toHaveBeenCalledWith('Write component tests');
    await waitFor(() => {
      expect(input).toHaveValue('');
    });
  });

  it('does not add a todo when the title is blank', () => {
    const addTodo = vi.fn();

    renderWithTodos(<TodoForm />, { addTodo });

    const input = screen.getByLabelText(/todo title/i);
    fireEvent.change(input, { target: { value: '   ' } });
    fireEvent.click(screen.getByRole('button', { name: /add todo/i }));

    expect(addTodo).not.toHaveBeenCalled();
    expect(input).toHaveValue('   ');
  });
});

describe('TodoList', () => {
  it('renders todos from context', () => {
    renderWithTodos(<TodoList />, {
      todos: [
        { id: '1', title: 'Write tests', completed: false },
        { id: '2', title: 'Ship UI', completed: true },
      ],
    });

    expect(screen.getByText('Write tests')).toBeInTheDocument();
    expect(screen.getByText('Ship UI')).toBeInTheDocument();
  });

  it('shows an empty state when there are no todos', () => {
    renderWithTodos(<TodoList />);

    expect(screen.getByText(/no todos yet/i)).toBeInTheDocument();
  });

  it('selects todos and bulk deletes selected ids', () => {
    const bulkDeleteTodos = vi.fn();

    renderWithTodos(<TodoList />, {
      todos: [
        { id: '1', title: 'Write tests', completed: false },
        { id: '2', title: 'Ship UI', completed: true },
      ],
      bulkDeleteTodos,
    });

    fireEvent.click(screen.getByRole('checkbox', { name: /select write tests/i }));
    fireEvent.click(screen.getByRole('button', { name: /delete selected \(1\)/i }));

    expect(bulkDeleteTodos).toHaveBeenCalledWith(['1']);
  });
});

describe('TodoItem', () => {
  it('renders title, completion checkbox, and delete button', () => {
    renderWithTodos(
      <TodoItem todo={{ id: '1', title: 'Review PR', completed: false }} />
    );

    expect(screen.getByText('Review PR')).toBeInTheDocument();
    expect(screen.getByRole('checkbox', { name: /mark review pr complete/i })).not.toBeChecked();
    expect(screen.getByRole('button', { name: /delete review pr/i })).toBeInTheDocument();
  });

  it('updates the todo when completion checkbox is clicked', () => {
    const updateTodo = vi.fn();

    renderWithTodos(
      <TodoItem todo={{ id: '1', title: 'Review PR', completed: false }} />,
      { updateTodo }
    );

    fireEvent.click(screen.getByRole('checkbox', { name: /mark review pr complete/i }));

    expect(updateTodo).toHaveBeenCalledWith('1', {
      title: 'Review PR',
      completed: true,
    });
  });

  it('deletes the todo when delete button is clicked', () => {
    const deleteTodo = vi.fn();

    renderWithTodos(
      <TodoItem todo={{ id: '1', title: 'Review PR', completed: false }} />,
      { deleteTodo }
    );

    fireEvent.click(screen.getByRole('button', { name: /delete review pr/i }));

    expect(deleteTodo).toHaveBeenCalledWith('1');
  });

  it('updates the todo title from edit mode', () => {
    const updateTodo = vi.fn();

    renderWithTodos(
      <TodoItem todo={{ id: '1', title: 'Review PR', completed: false }} />,
      { updateTodo }
    );

    fireEvent.click(screen.getByRole('button', { name: /edit review pr/i }));
    fireEvent.change(screen.getByLabelText(/edit todo title/i), {
      target: { value: 'Review issue' },
    });
    fireEvent.click(screen.getByRole('button', { name: /save todo/i }));

    expect(updateTodo).toHaveBeenCalledWith('1', {
      title: 'Review issue',
      completed: false,
    });
  });
});
