import { ColumnDef, OnChangeFn, PaginationState } from "@tanstack/react-table";

export type DataTableColumnDef<TData, TValue = unknown> = ColumnDef<
  TData,
  TValue
> & {
  width?: string;
};

export type DataTableProps<TData, TValue> = {
  columns: DataTableColumnDef<TData, TValue>[];
  data: TData[];
  pageCount: number;
  pageIndex: number;
  pageSize: number;
  isLoading?: boolean;
  onPaginationChange: OnChangeFn<PaginationState>;
  showPagination?: boolean;
  className?: string;
  noDataContent?: React.ReactNode;
};
