import { useState, useMemo } from "react";
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

function getMonthOptions() {
  const options: { value: string; label: string }[] = [];
  const now = new Date();

  // 11 months in the past + current month
  for (let i = 11; i >= 0; i--) {
    const d = new Date(now.getFullYear(), now.getMonth() - i, 1);
    const value = `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, "0")}`;
    const label = d.toLocaleDateString("pt-BR", { month: "long", year: "numeric" });
    options.push({ value, label: label.charAt(0).toUpperCase() + label.slice(1) });
  }

  // 12 months in the future
  for (let i = 1; i <= 12; i++) {
    const d = new Date(now.getFullYear(), now.getMonth() + i, 1);
    const value = `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, "0")}`;
    const label = d.toLocaleDateString("pt-BR", { month: "long", year: "numeric" });
    options.push({ value, label: label.charAt(0).toUpperCase() + label.slice(1) });
  }

  return options;
}

function parsePeriod(period: string) {
  const [year, month] = period.split("-").map(Number);
  const lastDay = new Date(Date.UTC(year, month, 0)).getUTCDate();
  return {
    startDate: `${year}-${String(month).padStart(2, "0")}-01`,
    endDate: `${year}-${String(month).padStart(2, "0")}-${String(lastDay).padStart(2, "0")}`,
  };
}

function isFuturePeriod(period: string): boolean {
  const now = new Date();
  const currentPeriod = `${now.getFullYear()}-${String(now.getMonth() + 1).padStart(2, "0")}`;
  return period > currentPeriod;
}

export default function DashboardPage() {
  const now = new Date();
  const currentPeriod = `${now.getFullYear()}-${String(now.getMonth() + 1).padStart(2, "0")}`;
  const [selectedPeriod, setSelectedPeriod] = useState(currentPeriod);
  const monthOptions = getMonthOptions();

  const { startDate, endDate } = parsePeriod(selectedPeriod);
  const isProjection = useMemo(() => isFuturePeriod(selectedPeriod), [selectedPeriod]);

  const { data, isLoading } = useQuery({
    queryKey: ["dashboard", selectedPeriod],
    queryFn: () => dashboardService.get({ startDate, endDate }),
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

  const activeBudgets = budgets.filter((b) => b.status !== undefined);

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-3">
          <h1 className="text-2xl font-bold text-gray-900">Dashboard</h1>
          {isProjection && (
            <span className="inline-flex items-center rounded-full bg-amber-100 px-3 py-1 text-xs font-semibold text-amber-700">
              Projeção
            </span>
          )}
        </div>
        <select
          value={selectedPeriod}
          onChange={(e) => setSelectedPeriod(e.target.value)}
          className="rounded-md border border-gray-300 bg-white px-3 py-2 text-sm text-gray-700 shadow-sm focus:border-primary-500 focus:outline-none focus:ring-1 focus:ring-primary-500"
        >
          {monthOptions.map((opt) => (
            <option key={opt.value} value={opt.value}>
              {opt.label}
            </option>
          ))}
        </select>
      </div>

      {isProjection && (
        <div className="rounded-lg border border-amber-200 bg-amber-50 px-4 py-3 text-sm text-amber-800">
          Você está visualizando uma projeção. Os valores exibidos correspondem apenas a registros já lançados para este período (parcelas futuras e registros recorrentes).
        </div>
      )}

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
              <p className="text-sm text-gray-500">{isProjection ? "Saldo Projetado" : "Saldo"}</p>
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
          {activeBudgets.length === 0 ? (
            <p className="text-sm text-gray-400">Nenhum orçamento ativo.</p>
          ) : (
            <div className="space-y-3">
              {activeBudgets.map((budget) => {
                const pct = budget.usedPercentage;
                const barWidth = Math.max(0, Math.min(pct, 100));
                const barColor =
                  pct >= 100 ? "bg-green-500"
                    : pct >= 80 ? "bg-emerald-400"
                    : pct >= 50 ? "bg-amber-400"
                    : pct >= 25 ? "bg-orange-400"
                    : "bg-red-500";
                const badgeVariant =
                  pct >= 100 ? "success" : pct >= 80 ? "warning" : "error";

                return (
                  <div key={budget.id} className="space-y-1">
                    <div className="flex items-center justify-between">
                      <span className="text-sm font-medium text-gray-700">{budget.name}</span>
                      <Badge variant={badgeVariant}>
                        {pct}%
                      </Badge>
                    </div>
                    <div className="h-2 overflow-hidden rounded-full bg-gray-100">
                      <div
                        className={`h-full rounded-full transition-all ${barColor}`}
                        style={{ width: `${barWidth}%` }}
                      />
                    </div>
                    <p className="text-xs text-gray-400">
                      {formatCurrency(budget.totalSpent)} / {formatCurrency(budget.goal)}
                    </p>
                  </div>
                );
              })}
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
        <h2 className="mb-4 text-lg font-semibold text-gray-900">
          {isProjection ? "Registros Projetados" : "Últimos Registros"}
        </h2>
        {latestRecords.length === 0 ? (
          <p className="text-sm text-gray-400">Nenhum registro encontrado.</p>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full text-left text-sm">
              <thead>
                <tr className="border-b text-gray-500">
                  <th className="pb-2 font-medium">Data</th>
                  <th className="pb-2 font-medium">Nome</th>
                  <th className="pb-2 font-medium">Tipo</th>
                  <th className="pb-2 font-medium">Valor</th>
                  <th className="pb-2 font-medium">Frequência</th>
                  <th className="pb-2 font-medium">Tags</th>
                </tr>
              </thead>
              <tbody>
                {latestRecords.map((record) => (
                  <tr key={record.id} className="border-b last:border-0">
                    <td className="py-3 text-gray-600">{formatDate(record.referenceDate)}</td>
                    <td className="py-3 font-medium text-gray-900">
                      {record.name || "-"}
                      {record.installmentTotal != null && record.installmentTotal > 1 && (
                        <span className="ml-2 inline-flex items-center rounded-full bg-blue-100 px-2 py-0.5 text-xs font-medium text-blue-700">
                          {record.installmentIndex}/{record.installmentTotal}
                        </span>
                      )}
                    </td>
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
