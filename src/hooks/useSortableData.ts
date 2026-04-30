import { useMemo, useState } from 'react';

type SortDirection = 'asc' | 'desc';

export type SortConfig<T> = {
  key: keyof T;
  direction: SortDirection;
} | null;

export function useSortableData<T>(items: T[]) {
  const [sortConfig, setSortConfig] = useState<SortConfig<T>>(null);

  const sortedItems = useMemo(() => {
    if (!sortConfig) {
      return items;
    }

    return [...items].sort((a, b) => {
      const aValue = a[sortConfig.key];
      const bValue = b[sortConfig.key];

      if (aValue === bValue) return 0;
      if (aValue === null || aValue === undefined) return 1;
      if (bValue === null || bValue === undefined) return -1;

      if (aValue < bValue) {
        return sortConfig.direction === 'asc' ? -1 : 1;
      }

      return sortConfig.direction === 'asc' ? 1 : -1;
    });
  }, [items, sortConfig]);

  function requestSort(key: keyof T) {
    setSortConfig((current) => {
      if (current?.key === key && current.direction === 'asc') {
        return { key, direction: 'desc' };
      }

      return { key, direction: 'asc' };
    });
  }

  function getSortLabel(key: keyof T) {
    if (sortConfig?.key !== key) {
      return '';
    }

    return sortConfig.direction === 'asc' ? ' ↑' : ' ↓';
  }

  return { sortedItems, requestSort, getSortLabel };
}