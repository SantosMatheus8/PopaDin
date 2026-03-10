import { useQuery } from "@tanstack/react-query";
import {
  TrendingUp,
  TrendingDown,
  Wallet,
  FileText,
} from "lucide-react";
import { dashboardService } from "../../services/dashboard";
import { Card } from "../../components/Card";
import { Badge } from "../../components/Badge";
import { EmptyState } from "../../components/EmptyState";
import { formatCurrency, formatDate } from "../../lib/format";
import { OperationLabels, FrequencyLabels, OperationEnum } from "../../types/enums";

export default function DashboardPage() {
  const { data, isLoading } = useQuery({
    queryKey: ["dashboard"],
    queryFn: () => dashboardService.get(),
  });

  if (isLoading) {
    return (
      <div className="flex h-64 items-center justify-center">
        <div className="h-8 w-8 animate-spin rounded-full border-4 border-primary-500 border-t-transparent" />
      </div>
    );
  }

  if (!data) return <EmptyState title="Erro ao carregar dashboard" />;

  const { summary, budgets, spendingByTag, latestRecords } = data;

  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-bold text-gray-900">Dashboard</h1>

      {/* Summary Cards */}
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4">
        <Card>
          <div className="flex items-center gap-3">
            <div className="rounded-lg bg-green-100 p-2">
              <TrendingUp className="h-5 w-5 text-green-600" />
            </div>
            <div>
              <p className="text-sm text-gray-500">Receitas</p>
              <p className="text-xl font-bold text-green-600">
                {formatCurrency(summary.totalDeposits)}
              </p>
            </div>
          </div>
        </Card>

        <Card>
          <div className="flex items-center gap-3">
            <div className="rounded-lg bg-red-100 p-2">
              <TrendingDown className="h-5 w-5 text-red-600" />
            </div>
            <div>
              <p className="text-sm text-gray-500">Despesas</p>
              <p className="text-xl font-bold text-red-600">
                {formatCurrency(summary.totalOutflows)}
              </p>
            </div>
          </div>
        </Card>

        <Card>
          <div className="flex items-center gap-3">
            <div className="rounded-lg bg-blue-100 p-2">
              <Wallet className="h-5 w-5 text-blue-600" />
            </div>
            <div>
              <p className="text-sm text-gray-500">Saldo</p>
              <p className={`text-xl font-bold ${summary.balance >= 0 ? "text-green-600" : "text-red-600"}`}>
                {formatCurrency(summary.balance)}
              </p>
            </div>
          </div>
        </Card>

        <Card>
          <div className="flex items-center gap-3">
            <div className="rounded-lg bg-purple-100 p-2">
              <FileText className="h-5 w-5 text-purple-600" />
            </div>
            <div>
              <p className="text-sm text-gray-500">Registros</p>
              <p className="text-xl font-bold text-gray-900">{summary.recordCount}</p>
            </div>
          </div>
        </Card>
      </div>

      <div className="grid grid-cols-1 gap-6 lg:grid-cols-2">
        {/* Budgets */}
        <Card>
          <h2 className="mb-4 text-lg font-semibold text-gray-900">Orçamentos</h2>
          {budgets.length === 0 ? (
            <p className="text-sm text-gray-400">Nenhum orçamento cadastrado.</p>
          ) : (
            <div className="space-y-3">
              {budgets.map((budget) => (
                <div key={budget.id} className="space-y-1">
                  <div className="flex items-center justify-between">
                    <span className="text-sm font-medium text-gray-700">{budget.name}</span>
                    <Badge
                      variant={
                        budget.status === "exceeded"
                          ? "error"
                          : budget.status === "alert"
                          ? "warning"
                          : "success"
                      }
                    >
                      {budget.usedPercentage}%
                    </Badge>
                  </div>
                  <div className="h-2 overflow-hidden rounded-full bg-gray-100">
                    <div
                      className={`h-full rounded-full transition-all ${
                        budget.status === "exceeded"
                          ? "bg-red-500"
                          : budget.status === "alert"
                          ? "bg-amber-500"
                          : "bg-green-500"
                      }`}
                      style={{ width: `${Math.min(budget.usedPercentage, 100)}%` }}
                    />
                  </div>
                  <p className="text-xs text-gray-400">
                    {formatCurrency(budget.totalSpent)} / {formatCurrency(budget.goal)}
                  </p>
                </div>
              ))}
            </div>
          )}
        </Card>

        {/* Spending by Tag */}
        <Card>
          <h2 className="mb-4 text-lg font-semibold text-gray-900">Gastos por Tag</h2>
          {spendingByTag.length === 0 ? (
            <p className="text-sm text-gray-400">Nenhum gasto registrado.</p>
          ) : (
            <div className="space-y-3">
              {spendingByTag.map((tag) => (
                <div key={tag.tagId} className="flex items-center justify-between">
                  <span className="text-sm text-gray-700">{tag.tagName}</span>
                  <span className="text-sm font-medium text-red-600">
                    {formatCurrency(tag.totalSpent)}
                  </span>
                </div>
              ))}
            </div>
          )}
        </Card>
      </div>

      {/* Latest Records */}
      <Card>
        <h2 className="mb-4 text-lg font-semibold text-gray-900">Últimos Registros</h2>
        {latestRecords.length === 0 ? (
          <p className="text-sm text-gray-400">Nenhum registro encontrado.</p>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full text-left text-sm">
              <thead>
                <tr className="border-b text-gray-500">
                  <th className="pb-2 font-medium">Data</th>
                  <th className="pb-2 font-medium">Tipo</th>
                  <th className="pb-2 font-medium">Valor</th>
                  <th className="pb-2 font-medium">Frequência</th>
                  <th className="pb-2 font-medium">Tags</th>
                </tr>
              </thead>
              <tbody>
                {latestRecords.map((record) => (
                  <tr key={record.id} className="border-b last:border-0">
                    <td className="py-3 text-gray-600">{formatDate(record.createdAt)}</td>
                    <td className="py-3">
                      <Badge variant={record.operation === OperationEnum.Deposit ? "success" : "error"}>
                        {OperationLabels[record.operation]}
                      </Badge>
                    </td>
                    <td className={`py-3 font-medium ${record.operation === OperationEnum.Deposit ? "text-green-600" : "text-red-600"}`}>
                      {formatCurrency(record.value)}
                    </td>
                    <td className="py-3 text-gray-600">{FrequencyLabels[record.frequency]}</td>
                    <td className="py-3">
                      <div className="flex flex-wrap gap-1">
                        {record.tags.map((tag) => (
                          <Badge key={tag.id} variant="info">{tag.name}</Badge>
                        ))}
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </Card>
    </div>
  );
}
