import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation } from "@tanstack/react-query";
import { User } from "lucide-react";
import toast from "react-hot-toast";
import { updateUserSchema, type UpdateUserFormData } from "../../schemas/user";
import { userService } from "../../services/user";
import { useAuth } from "../../hooks/useAuth";
import { useApiError } from "../../hooks/useApiError";
import { Card } from "../../components/Card";
import { Input } from "../../components/Input";
import { Button } from "../../components/Button";
import { formatCurrency, formatDate } from "../../lib/format";
import { useState } from "react";
import { ConfirmDialog } from "../../components/ConfirmDialog";
import { useNavigate } from "react-router-dom";

export default function ProfilePage() {
  const { user, refreshProfile, logout } = useAuth();
  const { handleError } = useApiError();
  const navigate = useNavigate();
  const [deleteOpen, setDeleteOpen] = useState(false);

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<UpdateUserFormData>({
    resolver: zodResolver(updateUserSchema),
    values: user ? { name: user.name, balance: user.balance, password: "" } : undefined,
  });

  const updateMutation = useMutation({
    mutationFn: (data: UpdateUserFormData) => {
      if (!user) throw new Error("User not found");
      const payload = {
        name: data.name,
        balance: data.balance,
        password: data.password || undefined,
      };
      return userService.update(user.id, payload);
    },
    onSuccess: () => {
      toast.success("Perfil atualizado!");
      refreshProfile();
    },
    onError: (err) => handleError(err, "Erro ao atualizar perfil"),
  });

  const deleteMutation = useMutation({
    mutationFn: () => {
      if (!user) throw new Error("User not found");
      return userService.delete(user.id);
    },
    onSuccess: () => {
      toast.success("Conta excluída!");
      logout();
      navigate("/login");
    },
    onError: (err) => handleError(err, "Erro ao excluir conta"),
  });

  if (!user) return null;

  return (
    <div className="mx-auto max-w-2xl space-y-6">
      <h1 className="text-2xl font-bold text-gray-900">Perfil</h1>

      <Card>
        <div className="mb-6 flex items-center gap-4">
          <div className="flex h-16 w-16 items-center justify-center rounded-full bg-primary-100">
            <User className="h-8 w-8 text-primary-700" />
          </div>
          <div>
            <h2 className="text-lg font-semibold text-gray-900">{user.name}</h2>
            <p className="text-sm text-gray-500">{user.email}</p>
            <p className="text-xs text-gray-400">
              Membro desde {formatDate(user.createdAt)} | Saldo: {formatCurrency(user.balance)}
            </p>
          </div>
        </div>

        <form onSubmit={handleSubmit((data) => updateMutation.mutate(data))} className="space-y-4">
          <Input
            label="Nome"
            error={errors.name?.message}
            {...register("name")}
          />
          <Input
            label="Saldo"
            type="number"
            step="0.01"
            error={errors.balance?.message}
            {...register("balance", { valueAsNumber: true })}
          />
          <Input
            label="Nova senha (deixe em branco para manter)"
            type="password"
            showPasswordToggle
            placeholder="********"
            error={errors.password?.message}
            {...register("password")}
          />
          <div className="flex justify-between pt-2">
            <Button
              type="button"
              variant="error"
              size="sm"
              onClick={() => setDeleteOpen(true)}
            >
              Excluir conta
            </Button>
            <Button type="submit" disabled={updateMutation.isPending}>
              {updateMutation.isPending ? "Salvando..." : "Salvar"}
            </Button>
          </div>
        </form>
      </Card>

      <ConfirmDialog
        isOpen={deleteOpen}
        onClose={() => setDeleteOpen(false)}
        onConfirm={() => deleteMutation.mutate()}
        title="Excluir conta"
        message="Tem certeza que deseja excluir sua conta? Todos os seus dados serão perdidos permanentemente."
        confirmLabel="Excluir minha conta"
        isLoading={deleteMutation.isPending}
      />
    </div>
  );
}
