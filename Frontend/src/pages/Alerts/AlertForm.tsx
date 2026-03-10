import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { alertSchema, type AlertFormData } from "../../schemas/alert";
import { Modal } from "../../components/Modal";
import { Input } from "../../components/Input";
import { Select } from "../../components/Select";
import { Button } from "../../components/Button";
import { AlertType, AlertTypeLabels } from "../../types/enums";

interface AlertFormProps {
  isOpen: boolean;
  onClose: () => void;
  onSubmit: (data: AlertFormData) => Promise<void>;
  isLoading?: boolean;
}

export function AlertForm({ isOpen, onClose, onSubmit, isLoading }: AlertFormProps) {
  const {
    register,
    handleSubmit,
    formState: { errors },
    reset,
  } = useForm<AlertFormData>({
    resolver: zodResolver(alertSchema),
    defaultValues: { type: AlertType.BALANCE_BELOW },
  });

  const handleFormSubmit = async (data: AlertFormData) => {
    await onSubmit(data);
    reset();
    onClose();
  };

  const typeOptions = Object.entries(AlertTypeLabels).map(([value, label]) => ({
    value,
    label,
  }));

  return (
    <Modal isOpen={isOpen} onClose={onClose} title="Novo Alerta">
      <form onSubmit={handleSubmit(handleFormSubmit)} className="space-y-4">
        <Select
          label="Tipo"
          options={typeOptions}
          error={errors.type?.message}
          {...register("type")}
        />
        <Input
          label="Valor Limite (R$)"
          type="number"
          step="0.01"
          placeholder="0.00"
          error={errors.threshold?.message}
          {...register("threshold", { valueAsNumber: true })}
        />
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
