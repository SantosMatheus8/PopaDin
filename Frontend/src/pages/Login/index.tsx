import { Button } from "../../components/Button";
import { Input } from "../../components/Input";
import { Mail, Lock } from "lucide-react";

export const Login = () => {
  return (
    <div className="flex items-center justify-center h-screen bg-white">
      <div className="h-screen hidden items-center justify-center bg-primary-500 lg:flex lg:w-1/2">
        <img src="login-image.png" className="w-3/5 h-3/5" />
      </div>
      <div className="w-full bg-gray-100 h-screen lg:w-1/2">
        <div className="flex flex-col justify-center gap-8 p-6 h-screen">
          <div>
            <h3 className="font-bold text-2xl text-center">LOGIN</h3>
          </div>
          <div className="flex flex-col gap-4">
            <Input placeholder="Digite seu e-mail" icon={<Mail />} />
            <Input
              showPasswordToggle
              placeholder="Digite sua senha"
              icon={<Lock />}
            />
            <div className="flex justify-end items-center">
              <p className=" text-blue-500 font-semibold cursor-pointer hover:text-blue-300">
                Esqueceu a senha?
              </p>
            </div>
            <Button className="w-full">Entrar</Button>
            <p>
              Não tem uma conta?{" "}
              <span className="text-blue-500 font-semibold cursor-pointer hover:text-blue-300">
                Inscrever-se
              </span>
            </p>
          </div>
        </div>
      </div>
    </div>
  );
};
