import { useEffect } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { goalSchema, type GoalFormData } from "../../schemas/goal";
import { Modal } from "../../components/Modal";
import { Input } from "../../components/Input";
import { Button } from "../../components/Button";
import type { GoalResponse } from "../../types";

interface GoalFormProps {
  isOpen: boolean;
  onClose: () => void;
  onSubmit: (data: GoalFormData) => Promise<void>;
  goal?: GoalResponse | null;
  isLoading?: boolean;
}

export function GoalForm({ isOpen, onClose, onSubmit, goal, isLoading }: GoalFormProps) {
  const {
    register,
    handleSubmit,
    formState: { errors },
    reset,
  } = useForm<GoalFormData>({
    resolver: zodResolver(goalSchema),
    defaultValues: { name: "", targetAmount: 0, deadline: null },
  });

  useEffect(() => {
    if (isOpen) {
      if (goal) {
        reset({
          name: goal.name,
          targetAmount: goal.targetAmount,
          deadline: goal.deadline ? goal.deadline.split("T")[0] : null,
        });
      } else {
        reset({ name: "", targetAmount: 0, deadline: null });
      }
    }
  }, [isOpen, goal, reset]);

  const handleFormSubmit = async (data: GoalFormData) => {
    await onSubmit(data);
    reset();
    onClose();
  };

  return (
    <Modal isOpen={isOpen} onClose={onClose} title={goal ? "Editar Meta" : "Nova Meta"}>
      <form onSubmit={handleSubmit(handleFormSubmit)} className="space-y-4">
        <Input
          label="Nome"
          placeholder="Nome da meta"
          error={errors.name?.message}
          {...register("name")}
        />
        <Input
          label="Valor da Meta (R$)"
          type="number"
          step="0.01"
          placeholder="0.00"
          error={errors.targetAmount?.message}
          {...register("targetAmount", { valueAsNumber: true })}
        />
        <Input
          label="Data Limite (opcional)"
          type="date"
          error={errors.deadline?.message}
          {...register("deadline")}
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
