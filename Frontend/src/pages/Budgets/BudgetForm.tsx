import { useEffect } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { budgetSchema, type BudgetFormData } from "../../schemas/budget";
import { Modal } from "../../components/Modal";
import { Input } from "../../components/Input";
import { Button } from "../../components/Button";
import type { BudgetResponse } from "../../types";

interface BudgetFormProps {
  isOpen: boolean;
  onClose: () => void;
  onSubmit: (data: BudgetFormData) => Promise<void>;
  budget?: BudgetResponse | null;
  isLoading?: boolean;
}

export function BudgetForm({ isOpen, onClose, onSubmit, budget, isLoading }: BudgetFormProps) {
  const {
    register,
    handleSubmit,
    formState: { errors },
    reset,
  } = useForm<BudgetFormData>({
    resolver: zodResolver(budgetSchema),
    defaultValues: { name: "", goal: 0 },
  });

  useEffect(() => {
    if (isOpen) {
      if (budget) {
        reset({ name: budget.name, goal: budget.goal });
      } else {
        reset({ name: "", goal: 0 });
      }
    }
  }, [isOpen, budget, reset]);

  const handleFormSubmit = async (data: BudgetFormData) => {
    await onSubmit(data);
    reset();
    onClose();
  };

  return (
    <Modal isOpen={isOpen} onClose={onClose} title={budget ? "Editar Orçamento" : "Novo Orçamento"}>
      <form onSubmit={handleSubmit(handleFormSubmit)} className="space-y-4">
        <Input
          label="Nome"
          placeholder="Nome do orçamento"
          error={errors.name?.message}
          {...register("name")}
        />
        <Input
          label="Meta (R$)"
          type="number"
          step="0.01"
          placeholder="0.00"
          error={errors.goal?.message}
          {...register("goal", { valueAsNumber: true })}
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
