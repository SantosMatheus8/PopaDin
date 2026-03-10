import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { Link, useNavigate } from "react-router-dom";
import { Wallet } from "lucide-react";
import toast from "react-hot-toast";
import { registerSchema, type RegisterFormData } from "../../schemas/auth";
import { userService } from "../../services/user";
import { useApiError } from "../../hooks/useApiError";
import { Input } from "../../components/Input";
import { Button } from "../../components/Button";
import { useState } from "react";

export default function RegisterPage() {
  const navigate = useNavigate();
  const { handleError } = useApiError();
  const [isLoading, setIsLoading] = useState(false);

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<RegisterFormData>({
    resolver: zodResolver(registerSchema),
    defaultValues: { balance: 0 },
  });

  const onSubmit = async (data: RegisterFormData) => {
    setIsLoading(true);
    try {
      await userService.create(data);
      toast.success("Conta criada com sucesso!");
      navigate("/login");
    } catch (error) {
      handleError(error, "Erro ao criar conta");
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="flex min-h-screen items-center justify-center bg-secondary-900 px-4">
      <div className="w-full max-w-md rounded-2xl bg-white p-8 shadow-xl">
        <div className="mb-8 flex flex-col items-center">
          <div className="mb-3 flex items-center gap-2">
            <Wallet className="h-8 w-8 text-primary-500" />
            <span className="text-2xl font-bold text-secondary-900">PopaDin</span>
          </div>
          <p className="text-sm text-gray-500">Crie sua conta</p>
        </div>

        <form onSubmit={handleSubmit(onSubmit)} className="space-y-5">
          <Input
            label="Nome"
            placeholder="Seu nome"
            error={errors.name?.message}
            {...register("name")}
          />
          <Input
            label="Email"
            type="email"
            placeholder="seu@email.com"
            error={errors.email?.message}
            {...register("email")}
          />
          <Input
            label="Senha"
            type="password"
            placeholder="Mínimo 6 caracteres"
            showPasswordToggle
            error={errors.password?.message}
            {...register("password")}
          />
          <Input
            label="Saldo inicial"
            type="number"
            step="0.01"
            placeholder="0.00"
            error={errors.balance?.message}
            {...register("balance", { valueAsNumber: true })}
          />
          <Button type="submit" className="w-full" disabled={isLoading}>
            {isLoading ? "Criando conta..." : "Criar conta"}
          </Button>
        </form>

        <p className="mt-6 text-center text-sm text-gray-500">
          Já tem conta?{" "}
          <Link to="/login" className="font-medium text-primary-500 hover:text-primary-700">
            Entre aqui
          </Link>
        </p>
      </div>
    </div>
  );
}
