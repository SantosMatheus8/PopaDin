import { useState } from "react";
import { SlidersHorizontal, X } from "lucide-react";
import { useQuery } from "@tanstack/react-query";
import { tagService } from "../../services/tag";
import { Input } from "../../components/Input";
import { Select } from "../../components/Select";
import { Button } from "../../components/Button";
import { OperationEnum, FrequencyEnum } from "../../types/enums";
import type { ListRecordsRequest } from "../../types";

interface RecordFiltersProps {
  params: ListRecordsRequest;
  onChange: (params: ListRecordsRequest) => void;
}

const EMPTY_FILTERS: Omit<ListRecordsRequest, "page" | "itemsPerPage"> = {
  name: undefined,
  operation: undefined,
  frequency: undefined,
  minValue: undefined,
  maxValue: undefined,
  tagIds: undefined,
  startDate: undefined,
  endDate: undefined,
};

export function RecordFilters({ params, onChange }: RecordFiltersProps) {
  const [open, setOpen] = useState(false);

  const { data: tagsData } = useQuery({
    queryKey: ["tags", "all"],
    queryFn: () => tagService.list({ itemsPerPage: 100 }),
    enabled: open,
  });

  const tags = tagsData?.lines ?? [];

  const hasActiveFilters = !!(
    params.name ||
    params.operation !== undefined ||
    params.frequency !== undefined ||
    params.minValue !== undefined ||
    params.maxValue !== undefined ||
    (params.tagIds && params.tagIds.length > 0) ||
    params.startDate ||
    params.endDate
  );

  function handleChange(field: keyof typeof EMPTY_FILTERS, value: unknown) {
    onChange({ ...params, page: 1, [field]: value === "" ? undefined : value });
  }

  function handleTagToggle(tagId: number) {
    const current = params.tagIds ?? [];
    const next = current.includes(tagId)
      ? current.filter((id) => id !== tagId)
      : [...current, tagId];
    onChange({ ...params, page: 1, tagIds: next.length > 0 ? next : undefined });
  }

  function handleClear() {
    onChange({ page: 1, itemsPerPage: params.itemsPerPage, ...EMPTY_FILTERS });
  }

  return (
    <div className="space-y-3">
      <div className="flex items-center gap-2">
        <Button
          variant={open ? "primary" : "outline"}
          size="sm"
          icon={<SlidersHorizontal className="h-4 w-4" />}
          onClick={() => setOpen((v) => !v)}
        >
          Filtros
          {hasActiveFilters && (
            <span className="ml-1.5 inline-flex h-4 w-4 items-center justify-center rounded-full bg-primary-600 text-[10px] text-white">
              !
            </span>
          )}
        </Button>
        {hasActiveFilters && (
          <button
            onClick={handleClear}
            className="flex items-center gap-1 text-xs text-gray-500 hover:text-red-500"
          >
            <X className="h-3 w-3" />
            Limpar filtros
          </button>
        )}
      </div>

      {open && (
        <div className="rounded-lg border border-gray-200 bg-gray-50 p-4">
          <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
            <Input
              label="Nome"
              placeholder="Buscar por nome..."
              value={params.name ?? ""}
              onChange={(e) => handleChange("name", e.target.value)}
              variant="outlined"
              color="primary"
              className="h-10"
            />

            <Select
              label="Tipo"
              value={params.operation ?? ""}
              onChange={(e) =>
                handleChange(
                  "operation",
                  e.target.value === "" ? undefined : Number(e.target.value)
                )
              }
              options={[
                { value: OperationEnum.Deposit, label: "Receita" },
                { value: OperationEnum.Outflow, label: "Despesa" },
              ]}
              placeholder="Todos"
              className="h-10"
            />

            <Select
              label="Frequência"
              value={params.frequency ?? ""}
              onChange={(e) =>
                handleChange(
                  "frequency",
                  e.target.value === "" ? undefined : Number(e.target.value)
                )
              }
              options={[
                { value: FrequencyEnum.Monthly, label: "Mensal" },
                { value: FrequencyEnum.Bimonthly, label: "Bimestral" },
                { value: FrequencyEnum.Quarterly, label: "Trimestral" },
                { value: FrequencyEnum.Semiannual, label: "Semestral" },
                { value: FrequencyEnum.Annual, label: "Anual" },
                { value: FrequencyEnum.OneTime, label: "Único" },
              ]}
              placeholder="Todas"
              className="h-10"
            />

            <Input
              label="Valor mínimo"
              type="number"
              min={0}
              step={0.01}
              placeholder="0,00"
              value={params.minValue ?? ""}
              onChange={(e) =>
                handleChange("minValue", e.target.value === "" ? undefined : Number(e.target.value))
              }
              variant="outlined"
              color="primary"
              className="h-10"
            />

            <Input
              label="Valor máximo"
              type="number"
              min={0}
              step={0.01}
              placeholder="0,00"
              value={params.maxValue ?? ""}
              onChange={(e) =>
                handleChange("maxValue", e.target.value === "" ? undefined : Number(e.target.value))
              }
              variant="outlined"
              color="primary"
              className="h-10"
            />

            <div className="space-y-2">
              <span className="block text-sm font-medium">Período</span>
              <div className="flex gap-2">
                <input
                  type="date"
                  value={params.startDate ?? ""}
                  onChange={(e) => handleChange("startDate", e.target.value || undefined)}
                  className="h-10 w-full rounded-md border-2 border-primary-500 bg-transparent px-3 text-sm focus:outline-none focus:ring-1 focus:ring-primary-900"
                />
                <input
                  type="date"
                  value={params.endDate ?? ""}
                  onChange={(e) => handleChange("endDate", e.target.value || undefined)}
                  className="h-10 w-full rounded-md border-2 border-primary-500 bg-transparent px-3 text-sm focus:outline-none focus:ring-1 focus:ring-primary-900"
                />
              </div>
            </div>
          </div>

          {tags.length > 0 && (
            <div className="mt-4 space-y-2">
              <span className="block text-sm font-medium">Tags</span>
              <div className="flex flex-wrap gap-2">
                {tags.map((tag) => {
                  const active = params.tagIds?.includes(tag.id!) ?? false;
                  return (
                    <button
                      key={tag.id}
                      type="button"
                      onClick={() => handleTagToggle(tag.id!)}
                      className="rounded-full border px-3 py-1 text-xs font-medium transition-colors"
                      style={
                        active && tag.color
                          ? { backgroundColor: tag.color + "30", borderColor: tag.color, color: tag.color }
                          : active
                          ? { backgroundColor: "#e0e7ff", borderColor: "#4f46e5", color: "#4f46e5" }
                          : { backgroundColor: "#fff", borderColor: "#d1d5db", color: "#6b7280" }
                      }
                    >
                      {tag.name}
                    </button>
                  );
                })}
              </div>
            </div>
          )}
        </div>
      )}
    </div>
  );
}
