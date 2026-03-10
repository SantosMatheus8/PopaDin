import { Link } from "react-router-dom";
import {
  LayoutDashboard,
  Receipt,
  Tags,
  Wallet,
  Bell,
  TrendingUp,
  ShieldCheck,
} from "lucide-react";
import { Card } from "../../components/Card";
import { useAuth } from "../../hooks/useAuth";

const features = [
  {
    to: "/dashboard",
    icon: LayoutDashboard,
    title: "Dashboard",
    description: "Visualize receitas, despesas e saldo do mês com filtros por período.",
    color: "bg-blue-100 text-blue-600",
  },
  {
    to: "/records",
    icon: Receipt,
    title: "Registros",
    description: "Gerencie todas as suas movimentações financeiras.",
    color: "bg-green-100 text-green-600",
  },
  {
    to: "/tags",
    icon: Tags,
    title: "Tags",
    description: "Organize seus registros com tags personalizadas e coloridas.",
    color: "bg-purple-100 text-purple-600",
  },
  {
    to: "/budgets",
    icon: Wallet,
    title: "Orçamentos",
    description: "Defina metas de gastos e acompanhe o progresso.",
    color: "bg-amber-100 text-amber-600",
  },
  {
    to: "/alerts",
    icon: Bell,
    title: "Alertas",
    description: "Receba notificações quando atingir limites financeiros.",
    color: "bg-red-100 text-red-600",
  },
];

export default function HomePage() {
  const { user } = useAuth();

  return (
    <div className="space-y-8">
      {/* Welcome Header */}
      <div className="rounded-2xl bg-gradient-to-r from-primary-600 to-primary-800 p-8 text-white shadow-lg">
        <div className="flex items-center gap-4">
          <div className="rounded-xl bg-white/20 p-3">
            <Wallet className="h-8 w-8 text-black/80" />
          </div>
          <div>
            <h1 className="text-3xl font-bold text-black/80">
              Bem-vindo ao PopaDin{user?.name ? `, ${user.name}` : ""}!
            </h1>
            <p className="mt-1 text-lg text-black/80">
              Seu gerenciador financeiro pessoal. Controle suas finanças de forma simples e eficiente.
            </p>
          </div>
        </div>
      </div>

      {/* Quick Stats */}
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-3">
        <Card>
          <div className="flex items-center gap-3">
            <div className="rounded-lg bg-green-100 p-2">
              <TrendingUp className="h-5 w-5 text-green-600" />
            </div>
            <div>
              <p className="text-sm text-gray-500">Acesso rápido</p>
              <Link to="/dashboard" className="text-sm font-semibold text-primary-600 hover:underline">
                Ver Dashboard →
              </Link>
            </div>
          </div>
        </Card>

        <Card>
          <div className="flex items-center gap-3">
            <div className="rounded-lg bg-blue-100 p-2">
              <Receipt className="h-5 w-5 text-blue-600" />
            </div>
            <div>
              <p className="text-sm text-gray-500">Novo registro</p>
              <Link to="/records" className="text-sm font-semibold text-primary-600 hover:underline">
                Criar Registro →
              </Link>
            </div>
          </div>
        </Card>

        <Card>
          <div className="flex items-center gap-3">
            <div className="rounded-lg bg-purple-100 p-2">
              <ShieldCheck className="h-5 w-5 text-purple-600" />
            </div>
            <div>
              <p className="text-sm text-gray-500">Controle</p>
              <Link to="/budgets" className="text-sm font-semibold text-primary-600 hover:underline">
                Ver Orçamentos →
              </Link>
            </div>
          </div>
        </Card>
      </div>

      {/* Features Grid */}
      <div>
        <h2 className="mb-4 text-lg font-semibold text-gray-900">Funcionalidades</h2>
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {features.map((feature) => (
            <Link key={feature.to} to={feature.to} className="group">
              <Card className="h-full transition-shadow group-hover:shadow-md">
                <div className="flex items-start gap-3">
                  <div className={`rounded-lg p-2 ${feature.color}`}>
                    <feature.icon className="h-5 w-5" />
                  </div>
                  <div>
                    <h3 className="font-semibold text-gray-900 group-hover:text-primary-600">
                      {feature.title}
                    </h3>
                    <p className="mt-1 text-sm text-gray-500">{feature.description}</p>
                  </div>
                </div>
              </Card>
            </Link>
          ))}
        </div>
      </div>
    </div>
  );
}
