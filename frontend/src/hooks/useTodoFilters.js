import { useState } from 'react';

export function useTodoFilters() {
  const [filters, setFilters] = useState({
    completed: null,
    search: undefined,
  });

  return {
    filters,
    setCompletedFilter: (completed) => setFilters(prev => ({ ...prev, completed })),
    setSearchFilter: (search) => setFilters(prev => ({ ...prev, search })),
  };
}
