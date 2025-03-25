import {
  flexRender,
  getCoreRowModel,
  getPaginationRowModel,
  useReactTable,
} from "@tanstack/react-table";
import {
  ChevronsLeft,
  ChevronLeft,
  ChevronsRight,
  ChevronRight,
  Loader2,
} from "lucide-react";
import { DataTableColumnDef, DataTableProps } from "./DataTable.types";

export function DataTable<TData, TValue>({
  columns,
  data,
  pageCount,
  pageIndex,
  pageSize,
  isLoading = false,
  onPaginationChange,
  showPagination = true,
  className,
  noDataContent,
}: DataTableProps<TData, TValue>) {
  const table = useReactTable({
    columns,
    data,
    manualPagination: true,
    pageCount,
    getCoreRowModel: getCoreRowModel(),
    getPaginationRowModel: getPaginationRowModel(),
    onPaginationChange,
    state: {
      pagination: {
        pageIndex,
        pageSize,
      },
    },
  });

  const totalItems = pageCount * pageSize;

  return (
    <div className={`space-y-4 ${className}`}>
      <div className="rounded-md border border-gray-300">
        <div className="relative w-full overflow-auto">
          <table className="w-full caption-bottom text-sm table-fixed">
            <thead>
              <tr className="border-b transition-colors border-gray-300">
                {table.getHeaderGroups()[0].headers.map((headerGroup) => {
                  const columnDef = headerGroup.column
                    .columnDef as DataTableColumnDef<TData, TValue>;
                  return (
                    <th
                      key={headerGroup.id}
                      className="h-12 px-4 text-left align-middle font-medium text-muted-foreground"
                      style={{
                        width: columnDef.width,
                        minWidth: columnDef.width,
                      }}
                    >
                      {flexRender(
                        headerGroup.column.columnDef.header,
                        headerGroup.getContext()
                      )}
                    </th>
                  );
                })}
              </tr>
            </thead>
            <tbody className="[&_tr:last-child]:border-0">
              {isLoading ? (
                <tr>
                  <td colSpan={columns.length} className="h-24 text-center">
                    <div className="flex justify-center items-center h-full">
                      <Loader2 className="h-6 w-6 animate-spin text-primary" />
                      <span className="ml-2">Carregando...</span>
                    </div>
                  </td>
                </tr>
              ) : data.length === 0 ? (
                <tr>
                  <td colSpan={columns.length} className="h-24 text-center">
                    {noDataContent ?? "Nenhum resultado encontrado"}
                  </td>
                </tr>
              ) : (
                table.getRowModel().rows.map((row) => (
                  <tr
                    key={row.id}
                    className="border-gray-300 border-b transition-colors hover:bg-gray-100"
                  >
                    {row.getVisibleCells().map((cell) => {
                      const columnDef = cell.column
                        .columnDef as DataTableColumnDef<TData, TValue>;
                      return (
                        <td
                          title={String(cell.getValue())}
                          key={cell.id}
                          className="p-4 align-middle truncate"
                          style={{
                            width: columnDef.width,
                            minWidth: columnDef.width,
                          }}
                        >
                          {flexRender(
                            cell.column.columnDef.cell,
                            cell.getContext()
                          )}
                        </td>
                      );
                    })}
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      </div>

      {showPagination && data.length > 0 && (
        <div className="flex items-center justify-between">
          <div className="flex-1 text-sm text-muted-foreground">
            {pageIndex * pageSize + 1} -{" "}
            {Math.min((pageIndex + 1) * pageSize, totalItems)} de {totalItems}{" "}
            itens
          </div>
          <div className="flex items-center space-x-4">
            <button
              onClick={() => table.setPageIndex(0)}
              disabled={!table.getCanPreviousPage() || isLoading}
              className="p-2 border border-gray-300 rounded-md disabled:cursor-not-allowed disabled:bg-gray-100 cursor-pointer"
            >
              <ChevronsLeft className="h-4 w-5" />
            </button>
            <button
              onClick={() => table.previousPage()}
              disabled={!table.getCanPreviousPage() || isLoading}
              className="p-2 border border-gray-300 rounded-md disabled:cursor-not-allowed disabled:bg-gray-100  cursor-pointer"
            >
              <ChevronLeft className="h-4 w-5" />
            </button>
            <div className="flex items-center gap-1">
              <span className="text-sm font-medium">Página</span>
              <span className="text-sm font-medium">
                {pageIndex + 1} de {Math.max(1, pageCount)}
              </span>
            </div>
            <button
              onClick={() => table.nextPage()}
              disabled={!table.getCanNextPage() || isLoading}
              className="p-2 border border-gray-300 rounded-md disabled:cursor-not-allowed disabled:gray-300 disabled:bg-gray-100 cursor-pointer"
            >
              <ChevronRight className="h-4 w-5" />
            </button>
            <button
              onClick={() => table.setPageIndex(pageCount - 1)}
              disabled={!table.getCanNextPage() || isLoading}
              className="p-2 border border-gray-300 rounded-md disabled:cursor-not-allowed disabled:gray-300 disabled:bg-gray-100 cursor-pointer"
            >
              <ChevronsRight className="h-4 w-5" />
            </button>
          </div>
        </div>
      )}
    </div>
  );
}
