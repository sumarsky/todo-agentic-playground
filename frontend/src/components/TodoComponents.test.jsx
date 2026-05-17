import { describe, it, expect, vi } from 'vitest';
import { useState } from 'react';
import { fireEvent, render, screen, waitFor } from '@testing-library/react';
import { TodoContext } from '../context/TodoContextValue';
import { TodoContextProvider } from '../context/TodoContext';
import { BulkDeleteButton } from './BulkDeleteButton';
import { FilterBar } from './FilterBar';
import { InlineAddForm } from './InlineAddForm';
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
    selectedIds: [],
    selectAll: vi.fn(),
    deselectAll: vi.fn(),
    selectTodo: vi.fn(),
    deselectTodo: vi.fn(),
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

  it('add button has tooltip title attribute', () => {
    renderWithTodos(
      <Toolbar onAddClick={vi.fn()} onSelectAll={vi.fn()} isAllSelected={false} />
    );

    const addButton = screen.getByRole('button', { name: /add todo/i });
    expect(addButton).toHaveAttribute('title', 'Add');
  });

  it('filter button has tooltip title attribute', () => {
    renderWithTodos(
      <Toolbar onAddClick={vi.fn()} onSelectAll={vi.fn()} isAllSelected={false} />
    );

    const filterButton = screen.getByRole('button', { name: /filter completed/i });
    expect(filterButton).toHaveAttribute('title', 'Toggle completed');
  });

  it('calls onSelectAll when select-all checkbox is clicked', () => {
    const onSelectAll = vi.fn();

    renderWithTodos(
      <Toolbar onAddClick={vi.fn()} onSelectAll={onSelectAll} isAllSelected={false} />
    );

    fireEvent.click(screen.getByRole('checkbox', { name: /select all/i }));

    expect(onSelectAll).toHaveBeenCalledTimes(1);
  });

  it('select-all checkbox shows indeterminate state when some todos selected', () => {
    renderWithTodos(
      <Toolbar onAddClick={vi.fn()} onSelectAll={vi.fn()} isAllSelected={false} isIndeterminate={true} />
    );

    const checkbox = screen.getByRole('checkbox', { name: /select all/i });
    expect(checkbox).toHaveProperty('indeterminate', true);
  });

  it('select-all checkbox is not indeterminate when all selected', () => {
    renderWithTodos(
      <Toolbar onAddClick={vi.fn()} onSelectAll={vi.fn()} isAllSelected={true} isIndeterminate={false} />
    );

    const checkbox = screen.getByRole('checkbox', { name: /select all/i });
    expect(checkbox).toHaveProperty('indeterminate', false);
    expect(checkbox).toBeChecked();
  });

  it('select-all checkbox is not indeterminate when none selected', () => {
    renderWithTodos(
      <Toolbar onAddClick={vi.fn()} onSelectAll={vi.fn()} isAllSelected={false} isIndeterminate={false} />
    );

    const checkbox = screen.getByRole('checkbox', { name: /select all/i });
    expect(checkbox).toHaveProperty('indeterminate', false);
    expect(checkbox).not.toBeChecked();
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

  it('has tooltip title attribute on filter button', () => {
    renderWithTodos(<FilterBar />, {
      filters: { completed: false, search: '' },
    });

    const button = screen.getByRole('button', { name: /show completed todos/i });
    expect(button).toHaveAttribute('title', 'Toggle completed');
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

  it('has tooltip title attribute', () => {
    renderWithTodos(<BulkDeleteButton selectedIds={['1', '3']} />);

    const button = screen.getByRole('button', { name: /delete selected \(2\)/i });
    expect(button).toHaveAttribute('title', 'Delete selected');
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

    const SelectableTodoList = () => {
      const [selectedIds, setSelectedIds] = useState([]);

      const ctx = {
        todos: [
          { id: '1', title: 'Write tests', completed: false },
          { id: '2', title: 'Ship UI', completed: true },
        ],
        loading: false,
        error: null,
        filters: { completed: false, search: '' },
        selectedIds,
        listTodos: vi.fn(),
        addTodo: vi.fn(),
        updateTodo: vi.fn(),
        deleteTodo: vi.fn(),
        bulkDeleteTodos,
        selectAll: vi.fn(),
        deselectAll: vi.fn(),
        selectTodo: (id) => { setSelectedIds((current) => [...current, id]); },
        deselectTodo: (id) => { setSelectedIds((current) => current.filter((todoId) => todoId !== id)); },
        setCompletedFilter: vi.fn(),
        setSearchFilter: vi.fn(),
      };

      return (
        <TodoContext.Provider value={ctx}>
          <TodoList />
        </TodoContext.Provider>
      );
    };

    render(<SelectableTodoList />);

    fireEvent.click(screen.getByRole('checkbox', { name: /select write tests/i }));
    fireEvent.click(screen.getByRole('button', { name: /delete selected \(1\)/i }));

    expect(bulkDeleteTodos).toHaveBeenCalledWith(['1']);
  });

  it('select-all checkbox selects all visible todos via context', () => {
    const selectAll = vi.fn();
    const deselectAll = vi.fn();

    renderWithTodos(<TodoList />, {
      todos: [
        { id: '1', title: 'Write tests', completed: false },
        { id: '2', title: 'Ship UI', completed: true },
      ],
      selectedIds: [],
      selectAll,
      deselectAll,
    });

    const selectAllCheckbox = screen.getByRole('checkbox', { name: /select all/i });
    fireEvent.click(selectAllCheckbox);

    expect(selectAll).toHaveBeenCalledTimes(1);
  });

  it('select-all checkbox deselects all when all are selected', () => {
    const selectAll = vi.fn();
    const deselectAll = vi.fn();

    renderWithTodos(<TodoList />, {
      todos: [
        { id: '1', title: 'Write tests', completed: false },
        { id: '2', title: 'Ship UI', completed: true },
      ],
      selectedIds: ['1', '2'],
      selectAll,
      deselectAll,
    });

    const selectAllCheckbox = screen.getByRole('checkbox', { name: /select all/i });
    expect(selectAllCheckbox).toBeChecked();

    fireEvent.click(selectAllCheckbox);

    expect(deselectAll).toHaveBeenCalledTimes(1);
  });

  it('select-all checkbox shows indeterminate when some todos selected', () => {
    renderWithTodos(<TodoList />, {
      todos: [
        { id: '1', title: 'Write tests', completed: false },
        { id: '2', title: 'Ship UI', completed: true },
        { id: '3', title: 'Docs', completed: false },
      ],
      selectedIds: ['1'],
      selectAll: vi.fn(),
      deselectAll: vi.fn(),
    });

    const selectAllCheckbox = screen.getByRole('checkbox', { name: /select all/i });
    expect(selectAllCheckbox).toHaveProperty('indeterminate', true);
  });

  it('shows inline add form as first list item when Add todo clicked', () => {
    const addTodo = vi.fn().mockResolvedValue({ id: '3', title: 'New todo', completed: false });

    renderWithTodos(<TodoList />, {
      todos: [
        { id: '1', title: 'Write tests', completed: false },
      ],
      addTodo,
    });

    fireEvent.click(screen.getByRole('button', { name: /add todo/i }));

    const input = screen.getByRole('textbox', { name: /todo title/i });
    expect(input).toBeInTheDocument();

    const listItems = screen.getByRole('list', { name: /todos/i }).children;
    expect(listItems[0].querySelector('input')).toBeTruthy();
  });

  it('submits todo and exits add mode on Enter', async () => {
    const addTodo = vi.fn().mockResolvedValue({ id: '3', title: 'New todo', completed: false });

    renderWithTodos(<TodoList />, {
      todos: [],
      addTodo,
    });

    fireEvent.click(screen.getByRole('button', { name: /add todo/i }));

    const input = screen.getByRole('textbox', { name: /todo title/i });
    fireEvent.change(input, { target: { value: 'New todo' } });
    fireEvent.keyDown(input, { key: 'Enter' });

    await waitFor(() => {
      expect(addTodo).toHaveBeenCalledWith('New todo');
    });
    expect(screen.queryByRole('textbox', { name: /todo title/i })).not.toBeInTheDocument();
  });

  it('exits add mode on Escape without submitting', () => {
    const addTodo = vi.fn();

    renderWithTodos(<TodoList />, {
      todos: [],
      addTodo,
    });

    fireEvent.click(screen.getByRole('button', { name: /add todo/i }));
    expect(screen.getByRole('textbox', { name: /todo title/i })).toBeInTheDocument();

    const input = screen.getByRole('textbox', { name: /todo title/i });
    fireEvent.keyDown(input, { key: 'Escape' });

    expect(addTodo).not.toHaveBeenCalled();
    expect(screen.queryByRole('textbox', { name: /todo title/i })).not.toBeInTheDocument();
  });
});

describe('InlineAddForm', () => {
  it('submits todo on Enter key', async () => {
    const onSubmit = vi.fn();
    const onCancel = vi.fn();

    render(<InlineAddForm onSubmit={onSubmit} onCancel={onCancel} />);

    const input = screen.getByRole('textbox', { name: /todo title/i });
    fireEvent.change(input, { target: { value: 'New todo' } });
    fireEvent.keyDown(input, { key: 'Enter' });

    expect(onSubmit).toHaveBeenCalledWith('New todo');
  });

  it('cancels on Escape key', () => {
    const onSubmit = vi.fn();
    const onCancel = vi.fn();

    render(<InlineAddForm onSubmit={onSubmit} onCancel={onCancel} />);

    const input = screen.getByRole('textbox', { name: /todo title/i });
    fireEvent.keyDown(input, { key: 'Escape' });

    expect(onCancel).toHaveBeenCalledTimes(1);
    expect(onSubmit).not.toHaveBeenCalled();
  });

  it('blocks empty input submit', () => {
    const onSubmit = vi.fn();
    const onCancel = vi.fn();

    render(<InlineAddForm onSubmit={onSubmit} onCancel={onCancel} />);

    const input = screen.getByRole('textbox', { name: /todo title/i });
    fireEvent.keyDown(input, { key: 'Enter' });

    expect(onSubmit).not.toHaveBeenCalled();
  });
});

describe('TodoItem', () => {
  it('renders title, delete button, and edit button', () => {
    renderWithTodos(
      <TodoItem todo={{ id: '1', title: 'Review PR', completed: false }} />
    );

    expect(screen.getByText('Review PR')).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /delete review pr/i })).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /edit review pr/i })).toBeInTheDocument();
  });

  it('delete button has tooltip title attribute', () => {
    renderWithTodos(
      <TodoItem todo={{ id: '1', title: 'Review PR', completed: false }} />
    );

    const deleteButton = screen.getByRole('button', { name: /delete review pr/i });
    expect(deleteButton).toHaveAttribute('title', 'Delete');
  });

  it('edit button has tooltip title attribute', () => {
    renderWithTodos(
      <TodoItem todo={{ id: '1', title: 'Review PR', completed: false }} />
    );

    const editButton = screen.getByRole('button', { name: /edit review pr/i });
    expect(editButton).toHaveAttribute('title', 'Edit');
  });

  it('save button in edit mode has tooltip title attribute', () => {
    renderWithTodos(
      <TodoItem todo={{ id: '1', title: 'Review PR', completed: false }} />
    );

    fireEvent.click(screen.getByRole('button', { name: /edit review pr/i }));

    const saveButton = screen.getByRole('button', { name: /save todo/i });
    expect(saveButton).toHaveAttribute('title', 'Save');
  });

  it('toggles completion when todo text is clicked', () => {
    const updateTodo = vi.fn();

    renderWithTodos(
      <TodoItem todo={{ id: '1', title: 'Review PR', completed: false }} />,
      { updateTodo }
    );

    fireEvent.click(screen.getByText('Review PR'));

    expect(updateTodo).toHaveBeenCalledWith('1', {
      title: 'Review PR',
      completed: true,
    });
  });

  it('toggles completion back when completed todo text is clicked', () => {
    const updateTodo = vi.fn();

    renderWithTodos(
      <TodoItem todo={{ id: '1', title: 'Review PR', completed: true }} />,
      { updateTodo }
    );

    fireEvent.click(screen.getByText('Review PR'));

    expect(updateTodo).toHaveBeenCalledWith('1', {
      title: 'Review PR',
      completed: false,
    });
  });

  it('applies completed styling when todo is done', () => {
    renderWithTodos(
      <TodoItem todo={{ id: '1', title: 'Done task', completed: true }} />
    );

    const span = screen.getByText('Done task');
    expect(span).toHaveStyle({
      opacity: '0.6',
      textDecoration: 'line-through',
      color: '#8a9478',
      backgroundColor: '#e8f0dc',
    });
  });

  it('does not apply completed styling when todo is not done', () => {
    renderWithTodos(
      <TodoItem todo={{ id: '1', title: 'Active task', completed: false }} />
    );

    const span = screen.getByText('Active task');
    expect(span).not.toHaveStyle({
      opacity: '0.6',
      textDecoration: 'line-through',
      color: '#8a9478',
      backgroundColor: '#e8f0dc',
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
