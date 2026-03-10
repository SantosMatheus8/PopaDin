import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { useQuery } from "@tanstack/react-query";
import { recordSchema, type RecordFormData } from "../../schemas/record";
import { tagService } from "../../services/tag";
import { Modal } from "../../components/Modal";
import { Input } from "../../components/Input";
import { Select } from "../../components/Select";
import { Button } from "../../components/Button";
import { OperationEnum, FrequencyEnum, OperationLabels, FrequencyLabels } from "../../types/enums";
import type { RecordResponse } from "../../types";

interface RecordFormProps {
  isOpen: boolean;
  onClose: () => void;
  onSubmit: (data: RecordFormData) => Promise<void>;
  record?: RecordResponse | null;
  isLoading?: boolean;
}

export function RecordForm({ isOpen, onClose, onSubmit, record, isLoading }: RecordFormProps) {
  const { data: tagsData } = useQuery({
    queryKey: ["tags", "all"],
    queryFn: () => tagService.list({ itemsPerPage: 100 }),
    enabled: isOpen,
  });

  const {
    register,
    handleSubmit,
    formState: { errors },
    reset,
    watch,
    setValue,
  } = useForm<RecordFormData>({
    resolver: zodResolver(recordSchema),
    defaultValues: record
      ? {
          operation: record.operation,
          value: record.value,
          frequency: record.frequency,
          tagIds: record.tags.map((t) => t.id).filter((id): id is number => id !== null),
        }
      : { operation: OperationEnum.Outflow, frequency: FrequencyEnum.Monthly, tagIds: [] },
  });

  const selectedTags = watch("tagIds");
  const tags = tagsData?.lines ?? [];

  const handleTagToggle = (tagId: number) => {
    const current = selectedTags || [];
    if (current.includes(tagId)) {
      setValue("tagIds", current.filter((id) => id !== tagId));
    } else {
      setValue("tagIds", [...current, tagId]);
    }
  };

  const handleFormSubmit = async (data: RecordFormData) => {
    await onSubmit(data);
    reset();
    onClose();
  };

  const operationOptions = Object.entries(OperationLabels).map(([value, label]) => ({
    value: Number(value),
    label,
  }));

  const frequencyOptions = Object.entries(FrequencyLabels).map(([value, label]) => ({
    value: Number(value),
    label,
  }));

  return (
    <Modal
      isOpen={isOpen}
      onClose={onClose}
      title={record ? "Editar Registro" : "Novo Registro"}
    >
      <form onSubmit={handleSubmit(handleFormSubmit)} className="space-y-4">
        <Select
          label="Tipo"
          options={operationOptions}
          error={errors.operation?.message}
          {...register("operation", { valueAsNumber: true })}
        />
        <Input
          label="Valor"
          type="number"
          step="0.01"
          placeholder="0.00"
          error={errors.value?.message}
          {...register("value", { valueAsNumber: true })}
        />
        <Select
          label="Frequência"
          options={frequencyOptions}
          error={errors.frequency?.message}
          {...register("frequency", { valueAsNumber: true })}
        />

        <div className="space-y-2">
          <label className="block text-sm font-medium">Tags</label>
          <div className="flex flex-wrap gap-2">
            {tags.map((tag) => (
              <button
                key={tag.id}
                type="button"
                onClick={() => tag.id && handleTagToggle(tag.id)}
                className={`rounded-full px-3 py-1 text-xs font-medium transition-colors ${
                  selectedTags?.includes(tag.id!)
                    ? "bg-primary-500 text-white"
                    : "bg-gray-100 text-gray-600 hover:bg-gray-200"
                }`}
              >
                {tag.name}
              </button>
            ))}
            {tags.length === 0 && (
              <span className="text-sm text-gray-400">Nenhuma tag disponível</span>
            )}
          </div>
        </div>

        <div className="flex justify-end gap-3 pt-2">
          <Button type="button" variant="outline" onClick={onClose}>
            Cancelar
          </Button>
          <Button type="submit" disabled={isLoading}>
            {isLoading ? "Salvando..." : "Salvar"}
          </Button>
        </div>
      </form>
    </Modal>
  );
}
