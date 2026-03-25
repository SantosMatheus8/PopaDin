import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation } from "@tanstack/react-query";
import { Camera, Trash2, User } from "lucide-react";
import toast from "react-hot-toast";
import { updateUserSchema, type UpdateUserFormData } from "../../schemas/user";
import { userService } from "../../services/user";
import { useAuth } from "../../hooks/useAuth";
import { useApiError } from "../../hooks/useApiError";
import { Card } from "../../components/Card";
import { Input } from "../../components/Input";
import { Button } from "../../components/Button";
import { formatCurrency, formatDate } from "../../lib/format";
import { useRef, useState } from "react";
import { ConfirmDialog } from "../../components/ConfirmDialog";
import { useNavigate } from "react-router-dom";

export default function ProfilePage() {
  const { user, refreshProfile, logout } = useAuth();
  const { handleError } = useApiError();
  const navigate = useNavigate();
  const [deleteOpen, setDeleteOpen] = useState(false);
  const [removePhotoOpen, setRemovePhotoOpen] = useState(false);
  const fileInputRef = useRef<HTMLInputElement>(null);

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<UpdateUserFormData>({
    resolver: zodResolver(updateUserSchema),
    values: user ? { name: user.name, password: "" } : undefined,
  });

  const updateMutation = useMutation({
    mutationFn: (data: UpdateUserFormData) => {
      if (!user) throw new Error("User not found");
      const payload = {
        name: data.name,
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

  const uploadPhotoMutation = useMutation({
    mutationFn: (file: File) => {
      if (!user) throw new Error("User not found");
      return userService.uploadProfilePicture(user.id, file);
    },
    onSuccess: () => {
      toast.success("Foto de perfil atualizada!");
      refreshProfile();
    },
    onError: (err) => handleError(err, "Erro ao atualizar foto de perfil"),
  });

  const deletePhotoMutation = useMutation({
    mutationFn: () => {
      if (!user) throw new Error("User not found");
      return userService.deleteProfilePicture(user.id);
    },
    onSuccess: () => {
      toast.success("Foto de perfil removida!");
      refreshProfile();
      setRemovePhotoOpen(false);
    },
    onError: (err) => handleError(err, "Erro ao remover foto de perfil"),
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

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;

    const allowedTypes = ["image/jpeg", "image/png", "image/webp"];
    if (!allowedTypes.includes(file.type)) {
      toast.error("Tipo de arquivo não permitido. Use JPG, PNG ou WebP.");
      return;
    }

    if (file.size > 5 * 1024 * 1024) {
      toast.error("Arquivo muito grande. Tamanho máximo: 5MB.");
      return;
    }

    uploadPhotoMutation.mutate(file);

    if (fileInputRef.current) {
      fileInputRef.current.value = "";
    }
  };

  if (!user) return null;

  return (
    <div className="mx-auto max-w-2xl space-y-6">
      <h1 className="text-2xl font-bold text-gray-900">Perfil</h1>

      <Card>
        <div className="mb-6 flex items-center gap-4">
          <div className="group relative">
            <div className="flex h-16 w-16 items-center justify-center overflow-hidden rounded-full bg-primary-100">
              {user.profilePictureUrl ? (
                <img
                  src={user.profilePictureUrl}
                  alt={user.name}
                  className="h-full w-full object-cover"
                />
              ) : (
                <User className="h-8 w-8 text-primary-700" />
              )}
            </div>
            <button
              type="button"
              onClick={() => fileInputRef.current?.click()}
              disabled={uploadPhotoMutation.isPending}
              className="absolute inset-0 flex items-center justify-center rounded-full bg-black/50 opacity-0 transition-opacity group-hover:opacity-100"
            >
              <Camera className="h-5 w-5 text-white" />
            </button>
            <input
              ref={fileInputRef}
              type="file"
              accept="image/jpeg,image/png,image/webp"
              onChange={handleFileChange}
              className="hidden"
            />
            {user.profilePictureUrl && (
              <button
                type="button"
                onClick={() => setRemovePhotoOpen(true)}
                className="absolute -right-1 -top-1 flex h-5 w-5 items-center justify-center rounded-full bg-red-500 text-white opacity-0 transition-opacity hover:bg-red-600 group-hover:opacity-100"
              >
                <Trash2 className="h-3 w-3" />
              </button>
            )}
          </div>
          <div>
            <h2 className="text-lg font-semibold text-gray-900">{user.name}</h2>
            <p className="text-sm text-gray-500">{user.email}</p>
            <p className="text-xs text-gray-400">
              Membro desde {formatDate(user.createdAt)} | Saldo: {formatCurrency(user.balance)}
            </p>
            {uploadPhotoMutation.isPending && (
              <p className="text-xs text-primary-600">Enviando foto...</p>
            )}
          </div>
        </div>

        <form onSubmit={handleSubmit((data) => updateMutation.mutate(data))} className="space-y-4">
          <Input
            label="Nome"
            error={errors.name?.message}
            {...register("name")}
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
        isOpen={removePhotoOpen}
        onClose={() => setRemovePhotoOpen(false)}
        onConfirm={() => deletePhotoMutation.mutate()}
        title="Remover foto de perfil"
        message="Tem certeza que deseja remover sua foto de perfil?"
        confirmLabel="Remover foto"
        isLoading={deletePhotoMutation.isPending}
      />

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
