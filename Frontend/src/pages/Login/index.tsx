import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { Link, useNavigate } from "react-router-dom";
import { Wallet } from "lucide-react";
import toast from "react-hot-toast";
import { loginSchema, type LoginFormData } from "../../schemas/auth";
import { authService } from "../../services/auth";
import { useAuth } from "../../hooks/useAuth";
import { useApiError } from "../../hooks/useApiError";
import { Input } from "../../components/Input";
import { Button } from "../../components/Button";
import { useState } from "react";

export default function LoginPage() {
  const navigate = useNavigate();
  const { login } = useAuth();
  const { handleError } = useApiError();
  const [isLoading, setIsLoading] = useState(false);

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<LoginFormData>({
    resolver: zodResolver(loginSchema),
  });

  const onSubmit = async (data: LoginFormData) => {
    setIsLoading(true);
    try {
      const response = await authService.login(data);
      await login(response.access_token);
      toast.success("Login realizado com sucesso!");
      navigate("/");
    } catch (error) {
      handleError(error, "Credenciais inválidas");
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
          <p className="text-sm text-gray-500">Entre na sua conta</p>
        </div>

        <form onSubmit={handleSubmit(onSubmit)} className="space-y-5">
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
            placeholder="********"
            showPasswordToggle
            error={errors.password?.message}
            {...register("password")}
          />
          <Button type="submit" className="w-full" disabled={isLoading}>
            {isLoading ? "Entrando..." : "Entrar"}
          </Button>
        </form>

        <p className="mt-6 text-center text-sm text-gray-500">
          Não tem conta?{" "}
          <Link to="/register" className="font-medium text-primary-500 hover:text-primary-700">
            Cadastre-se
          </Link>
        </p>
      </div>
    </div>
  );
}
