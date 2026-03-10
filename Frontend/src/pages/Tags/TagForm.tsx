import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { tagSchema, type TagFormData } from "../../schemas/tag";
import { Modal } from "../../components/Modal";
import { Input } from "../../components/Input";
import { Select } from "../../components/Select";
import { Textarea } from "../../components/Textarea";
import { Button } from "../../components/Button";
import { OperationLabels } from "../../types/enums";
import type { TagResponse } from "../../types";

interface TagFormProps {
  isOpen: boolean;
  onClose: () => void;
  onSubmit: (data: TagFormData) => Promise<void>;
  tag?: TagResponse | null;
  isLoading?: boolean;
}

export function TagForm({ isOpen, onClose, onSubmit, tag, isLoading }: TagFormProps) {
  const {
    register,
    handleSubmit,
    formState: { errors },
    reset,
  } = useForm<TagFormData>({
    resolver: zodResolver(tagSchema),
    defaultValues: tag
      ? { name: tag.name, tagType: tag.tagType, description: tag.description }
      : { name: "", tagType: null, description: "" },
  });

  const handleFormSubmit = async (data: TagFormData) => {
    await onSubmit(data);
    reset();
    onClose();
  };

  const operationOptions = [
    { value: "", label: "Nenhum" },
    ...Object.entries(OperationLabels).map(([value, label]) => ({
      value: Number(value),
      label,
    })),
  ];

  return (
    <Modal isOpen={isOpen} onClose={onClose} title={tag ? "Editar Tag" : "Nova Tag"}>
      <form onSubmit={handleSubmit(handleFormSubmit)} className="space-y-4">
        <Input
          label="Nome"
          placeholder="Nome da tag"
          error={errors.name?.message}
          {...register("name")}
        />
        <Select
          label="Tipo"
          options={operationOptions}
          error={errors.tagType?.message}
          {...register("tagType", { valueAsNumber: true })}
        />
        <Textarea
          label="Descrição"
          placeholder="Descrição opcional"
          error={errors.description?.message}
          {...register("description")}
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
