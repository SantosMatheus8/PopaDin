import {
  flexRender,
  getCoreRowModel,
  getPaginationRowModel,
  useReactTable,
} from "@tanstack/react-table";

import { ChevronLeft, ChevronRight } from "lucide-react";
import { DataTableColumnDef, DataTableProps } from "./DataTable.types";

export function DataTable<TData, TValue>({
  columns,
  data,
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

  const currentPage = table.getState().pagination.pageIndex + 1;

  return (
    <div className={`space-y-4 ${className}`}>
      <div className="rounded-md">
        <div className="relative w-full overflow-auto">
          <table className="w-full caption-bottom text-sm table-fixed">
            <thead className="bg-gray-100 uppercase">
              <tr>
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
                        textAlign: columnDef.align,
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
            <tbody>
              {isLoading ? (
                <tr>
                  <td colSpan={columns.length} className="h-24 text-center">
                    <div className="flex justify-center items-center h-full">
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
                    className="border-gray-300 border-b transition-colors"
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
                            textAlign: columnDef.align,
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

      {showPagination && (
        <div className="flex flex-col sm:flex-row items-start sm:items-center gap-4">
          <div className="flex items-center order-1 sm:order-1">
            <nav
              className="flex items-center space-x-1 gap-4"
              aria-label="Paginação"
            >
              <button
                className="text-white bg-black w-8 h-8 flex items-center justify-center disabled:bg-gray-300 hover:bg-gray-500 disabled:cursor-default cursor-pointer"
                onClick={() => table.previousPage()}
                disabled={!table.getCanPreviousPage()}
              >
                <ChevronLeft />
              </button>
              {Array.from({ length: table.getPageCount() }, (_, index) => (
                <button
                  key={index}
                  className={`w-8 h-8 flex items-center justify-center disabled:bg-primary-100  ${
                    currentPage === index + 1
                      ? "bg-black text-white cursor-default"
                      : "hover:bg-gray-100 cursor-pointer"
                  }`}
                  onClick={() => table.setPageIndex(index)}
                >
                  {index + 1}
                </button>
              ))}
              <button
                className="text-white bg-black w-8 h-8 flex items-center justify-center disabled:bg-gray-300 hover:bg-gray-500 disabled:cursor-default cursor-pointer"
                onClick={() => table.nextPage()}
                disabled={!table.getCanNextPage()}
              >
                <ChevronRight />
              </button>
            </nav>
          </div>
        </div>
      )}
    </div>
  );
}
