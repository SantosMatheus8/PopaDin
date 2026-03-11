import { useEffect, useState } from "react";
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

  const today = new Date().toISOString().slice(0, 10);
  const [isInstallment, setIsInstallment] = useState(false);

  const {
    register,
    handleSubmit,
    formState: { errors },
    reset,
    watch,
    setValue,
  } = useForm<RecordFormData>({
    resolver: zodResolver(recordSchema),
    defaultValues: { name: "", operation: OperationEnum.Outflow, frequency: FrequencyEnum.Monthly, tagIds: [], referenceDate: today },
  });

  useEffect(() => {
    if (isOpen) {
      if (record) {
        const hasInstallment = record.installmentTotal != null && record.installmentTotal > 1;
        setIsInstallment(hasInstallment);
        reset({
          name: record.name,
          operation: record.operation,
          value: hasInstallment ? record.value * record.installmentTotal! : record.value,
          frequency: record.frequency,
          tagIds: record.tags.map((t) => t.id).filter((id): id is number => id !== null),
          referenceDate: record.referenceDate ? record.referenceDate.slice(0, 10) : today,
          installments: hasInstallment ? record.installmentTotal! : undefined,
          recurrenceEndDate: record.recurrenceEndDate ? record.recurrenceEndDate.slice(0, 10) : undefined,
        });
      } else {
        setIsInstallment(false);
        reset({ name: "", operation: OperationEnum.Outflow, frequency: FrequencyEnum.Monthly, tagIds: [], referenceDate: today });
      }
    }
  }, [isOpen, record, reset, today]);

  const selectedTags = watch("tagIds");
  const selectedFrequency = watch("frequency");
  const tags = tagsData?.lines ?? [];
  const canInstallment = Number(selectedFrequency) === FrequencyEnum.OneTime;

  // Reset installment if frequency changed to non-OneTime
  useEffect(() => {
    if (!canInstallment && isInstallment) {
      setIsInstallment(false);
      setValue("installments", undefined);
    }
  }, [canInstallment, isInstallment, setValue]);

  const handleTagToggle = (tagId: number) => {
    const current = selectedTags || [];
    if (current.includes(tagId)) {
      setValue("tagIds", current.filter((id) => id !== tagId));
    } else {
      setValue("tagIds", [...current, tagId]);
    }
  };

  const handleInstallmentToggle = () => {
    const next = !isInstallment;
    setIsInstallment(next);
    if (!next) {
      setValue("installments", undefined);
    } else {
      setValue("installments", 2);
    }
  };

  const handleFormSubmit = async (data: RecordFormData) => {
    const submitData = {
      ...data,
      referenceDate: data.referenceDate || undefined,
      installments: isInstallment ? data.installments : undefined,
      recurrenceEndDate: undefined,
    };
    await onSubmit(submitData);
    reset();
    setIsInstallment(false);
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
        <Input
          label="Nome"
          placeholder="Nome do registro"
          error={errors.name?.message}
          {...register("name")}
        />
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
        <Input
          label="Data de Referência"
          type="date"
          error={errors.referenceDate?.message}
          {...register("referenceDate")}
        />

        {/* Installment toggle - only for OneTime frequency */}
        {canInstallment && (
          <div className="space-y-2">
            <label className="flex items-center gap-2 cursor-pointer">
              <input
                type="checkbox"
                checked={isInstallment}
                onChange={handleInstallmentToggle}
                className="h-4 w-4 rounded border-gray-300 text-primary-500 focus:ring-primary-500"
              />
              <span className="text-sm font-medium text-gray-700">Parcelar</span>
            </label>
            {isInstallment && (
              <Input
                label="Número de parcelas"
                type="number"
                min={2}
                max={48}
                placeholder="2"
                error={errors.installments?.message}
                {...register("installments", { valueAsNumber: true })}
              />
            )}
          </div>
        )}

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
                style={
                  tag.color && !selectedTags?.includes(tag.id!)
                    ? { backgroundColor: tag.color + "20", color: tag.color, borderColor: tag.color, borderWidth: 1 }
                    : tag.color && selectedTags?.includes(tag.id!)
                    ? { backgroundColor: tag.color, color: "#fff" }
                    : undefined
                }
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
