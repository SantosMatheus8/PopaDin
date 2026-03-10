import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { exportRecordsSchema, type ExportRecordsFormData } from "../../schemas/record";
import { Modal } from "../../components/Modal";
import { Input } from "../../components/Input";
import { Button } from "../../components/Button";

interface ExportModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSubmit: (data: ExportRecordsFormData) => Promise<void>;
  isLoading?: boolean;
}

export function ExportModal({ isOpen, onClose, onSubmit, isLoading }: ExportModalProps) {
  const today = new Date().toISOString().slice(0, 10);
  const firstDayOfMonth = new Date(new Date().getFullYear(), new Date().getMonth(), 1).toISOString().slice(0, 10);

  const {
    register,
    handleSubmit,
    formState: { errors },
    reset,
  } = useForm<ExportRecordsFormData>({
    resolver: zodResolver(exportRecordsSchema),
    defaultValues: { startDate: firstDayOfMonth, endDate: today },
  });

  const handleFormSubmit = async (data: ExportRecordsFormData) => {
    await onSubmit({
      startDate: `${data.startDate}T00:00:00.000Z`,
      endDate: `${data.endDate}T23:59:59.999Z`,
    });
    reset();
    onClose();
  };

  return (
    <Modal isOpen={isOpen} onClose={onClose} title="Exportar Registros">
      <form onSubmit={handleSubmit(handleFormSubmit)} className="space-y-4">
        <Input
          label="Data inicial"
          type="date"
          error={errors.startDate?.message}
          {...register("startDate")}
        />
        <Input
          label="Data final"
          type="date"
          error={errors.endDate?.message}
          {...register("endDate")}
        />
        <div className="flex justify-end gap-3 pt-2">
          <Button type="button" variant="outline" onClick={onClose}>
            Cancelar
          </Button>
          <Button type="submit" disabled={isLoading}>
            {isLoading ? "Exportando..." : "Exportar"}
          </Button>
        </div>
      </form>
    </Modal>
  );
}
