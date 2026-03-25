import { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import {
  TrendingUp,
  TrendingDown,
  BarChart3,
  AlertTriangle,
  Target,
  RefreshCw,
  ArrowUpRight,
  ArrowDownRight,
  Minus,
} from "lucide-react";
import { Card } from "../../components/Card";
import { Badge } from "../../components/Badge";
import { EmptyState } from "../../components/EmptyState";
import { formatCurrency, formatDateTime } from "../../lib/format";
import {
  analyticsService,
  InsightResponse,
  ForecastData,
  SpendingTrendData,
  MonthlyComparisonData,
  AnomalyData,
} from "../../services/analytics";
import toast from "react-hot-toast";

const INSIGHT_TYPE_LABELS: Record<string, string> = {
  SPENDING_TREND: "Tendência de Gastos",
  BALANCE_FORECAST: "Previsão de Saldo",
  MONTHLY_COMPARISON: "Comparativo Mensal",
  ANOMALY_DETECTION: "Gastos Atípicos",
};

const INSIGHT_TYPE_ICONS: Record<string, React.ReactNode> = {
  SPENDING_TREND: <TrendingUp className="h-5 w-5" />,
  BALANCE_FORECAST: <Target className="h-5 w-5" />,
  MONTHLY_COMPARISON: <BarChart3 className="h-5 w-5" />,
  ANOMALY_DETECTION: <AlertTriangle className="h-5 w-5" />,
};

const SEVERITY_BADGE: Record<string, "info" | "warning" | "error"> = {
  info: "info",
  warning: "warning",
  critical: "error",
};

const SEVERITY_LABELS: Record<string, string> = {
  info: "Normal",
  warning: "Atenção",
  critical: "Crítico",
};

type FilterType = "ALL" | "SPENDING_TREND" | "BALANCE_FORECAST" | "MONTHLY_COMPARISON" | "ANOMALY_DETECTION";

export default function AnalyticsPage() {
  const [filterType, setFilterType] = useState<FilterType>("ALL");
  const queryClient = useQueryClient();

  const { data: latestInsights, isLoading: loadingLatest } = useQuery({
    queryKey: ["analytics", "latest"],
    queryFn: () => analyticsService.getLatestInsights(),
  });

  const { data: forecastData, isLoading: loadingForecast } = useQuery({
    queryKey: ["analytics", "forecast"],
    queryFn: () => analyticsService.getForecast(),
  });

  const { data: insightsList, isLoading: loadingList } = useQuery({
    queryKey: ["analytics", "insights", filterType],
    queryFn: () =>
      analyticsService.getInsights(
        filterType === "ALL" ? undefined : filterType,
        1,
        50
      ),
  });

  const refreshMutation = useMutation({
    mutationFn: () => analyticsService.refreshInsights(),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["analytics"] });
      toast.success("Insights atualizados com sucesso!");
    },
    onError: () => {
      toast.error("Erro ao atualizar insights");
    },
  });

  const isLoading = loadingLatest || loadingForecast || loadingList;

  if (isLoading) {
    return (
      <div className="flex h-64 items-center justify-center">
        <div className="h-8 w-8 animate-spin rounded-full border-4 border-primary-500 border-t-transparent" />
      </div>
    );
  }

  const forecast = forecastData?.data as ForecastData | undefined;

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold text-gray-900">Insights Financeiros</h1>
        <button
          onClick={() => refreshMutation.mutate()}
          disabled={refreshMutation.isPending}
          className="flex items-center gap-2 rounded-lg bg-green-600 px-4 py-2 text-sm font-medium text-white transition-colors hover:bg-green-700 disabled:opacity-50"
        >
          <RefreshCw
            className={`h-4 w-4 ${refreshMutation.isPending ? "animate-spin" : ""}`}
          />
          Atualizar Insights
        </button>
      </div>

      {/* Summary Cards */}
      {latestInsights && latestInsights.length > 0 && (
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4">
          {latestInsights.map((insight) => (
            <SummaryCard key={insight._id} insight={insight} />
          ))}
        </div>
      )}

      {/* Balance Forecast Detail */}
      {forecast && <ForecastCard forecast={forecast} />}

      {/* Filter Tabs */}
      <div className="flex gap-2 overflow-x-auto">
        {(["ALL", "SPENDING_TREND", "BALANCE_FORECAST", "MONTHLY_COMPARISON", "ANOMALY_DETECTION"] as FilterType[]).map(
          (type) => (
            <button
              key={type}
              onClick={() => setFilterType(type)}
              className={`whitespace-nowrap rounded-lg px-3 py-2 text-sm font-medium transition-colors ${
                filterType === type
                  ? "bg-primary-600 text-white"
                  : "bg-gray-100 text-gray-600 hover:bg-gray-200"
              }`}
            >
              {type === "ALL" ? "Todos" : INSIGHT_TYPE_LABELS[type]}
            </button>
          )
        )}
      </div>

      {/* Insights List */}
      {!insightsList?.data?.length ? (
        <EmptyState
          title="Nenhum insight encontrado"
          description="Os insights serão gerados automaticamente conforme você registra suas movimentações financeiras. Clique em 'Atualizar Insights' para gerar agora."
        />
      ) : (
        <div className="space-y-4">
          {insightsList.data.map((insight) => (
            <InsightCard key={insight._id} insight={insight} />
          ))}
        </div>
      )}
    </div>
  );
}

function SummaryCard({ insight }: { insight: InsightResponse }) {
  const iconColors: Record<string, string> = {
    SPENDING_TREND: "bg-purple-100 text-purple-600",
    BALANCE_FORECAST: "bg-blue-100 text-blue-600",
    MONTHLY_COMPARISON: "bg-green-100 text-green-600",
    ANOMALY_DETECTION: "bg-amber-100 text-amber-600",
  };

  return (
    <Card>
      <div className="flex items-start gap-3">
        <div className={`rounded-lg p-2 ${iconColors[insight.type] ?? "bg-gray-100 text-gray-600"}`}>
          {INSIGHT_TYPE_ICONS[insight.type]}
        </div>
        <div className="min-w-0 flex-1">
          <p className="text-xs text-gray-500">
            {INSIGHT_TYPE_LABELS[insight.type] ?? insight.type}
          </p>
          <p className="mt-0.5 truncate text-sm font-semibold text-gray-900">
            {insight.title}
          </p>
          <Badge
            variant={SEVERITY_BADGE[insight.severity] ?? "default"}
            className="mt-1"
          >
            {SEVERITY_LABELS[insight.severity] ?? insight.severity}
          </Badge>
        </div>
      </div>
    </Card>
  );
}

function ForecastCard({ forecast }: { forecast: ForecastData }) {
  return (
    <Card>
      <h2 className="mb-4 text-lg font-semibold text-gray-900">
        Previsão de Saldo
      </h2>
      <div className="grid grid-cols-1 gap-6 sm:grid-cols-2">
        {/* Current Balance & Projections */}
        <div className="space-y-4">
          <div>
            <p className="text-sm text-gray-500">Saldo ao Final do Mês</p>
            <p
              className={`text-2xl font-bold ${
                forecast.currentBalance >= 0
                  ? "text-green-600"
                  : "text-red-600"
              }`}
            >
              {formatCurrency(forecast.currentBalance)}
            </p>
          </div>
          <div className="space-y-2">
            {forecast.forecast.map((f) => (
              <div
                key={f.months}
                className="flex items-center justify-between rounded-lg bg-gray-50 px-3 py-2"
              >
                <span className="text-sm text-gray-600">
                  Em {f.months} {f.months === 1 ? "mês" : "meses"}
                </span>
                <span
                  className={`text-sm font-semibold ${
                    f.projected >= 0 ? "text-green-600" : "text-red-600"
                  }`}
                >
                  {formatCurrency(f.projected)}
                </span>
              </div>
            ))}
          </div>
          {forecast.goesNegativeInMonths != null && forecast.goesNegativeInMonths >= 0 && (
            <div className="rounded-lg border border-red-200 bg-red-50 px-3 py-2 text-sm text-red-700">
              {forecast.goesNegativeInMonths === 0 ? (
                <strong>Seu saldo está negativo</strong>
              ) : (
                <>
                  Saldo pode ficar negativo em{" "}
                  <strong>{forecast.goesNegativeInMonths} meses</strong>
                </>
              )}
            </div>
          )}
        </div>

        {/* Monthly Breakdown */}
        <div className="space-y-3">
          <div className="flex items-center justify-between rounded-lg bg-green-50 px-3 py-2">
            <div className="flex items-center gap-2">
              <ArrowUpRight className="h-4 w-4 text-green-600" />
              <span className="text-sm text-gray-700">Receita Recorrente</span>
            </div>
            <span className="text-sm font-semibold text-green-600">
              {formatCurrency(forecast.monthlyRecurringIncome)}
            </span>
          </div>
          <div className="flex items-center justify-between rounded-lg bg-red-50 px-3 py-2">
            <div className="flex items-center gap-2">
              <ArrowDownRight className="h-4 w-4 text-red-600" />
              <span className="text-sm text-gray-700">
                Despesa Recorrente
              </span>
            </div>
            <span className="text-sm font-semibold text-red-600">
              {formatCurrency(forecast.monthlyRecurringExpenses)}
            </span>
          </div>
          <div className="flex items-center justify-between rounded-lg bg-amber-50 px-3 py-2">
            <div className="flex items-center gap-2">
              <Minus className="h-4 w-4 text-amber-600" />
              <span className="text-sm text-gray-700">
                Média Gastos Avulsos
              </span>
            </div>
            <span className="text-sm font-semibold text-amber-600">
              {formatCurrency(forecast.averageOneTimeExpenses)}
            </span>
          </div>
        </div>
      </div>
    </Card>
  );
}

function InsightCard({ insight }: { insight: InsightResponse }) {
  return (
    <Card>
      <div className="flex items-start gap-4">
        <div className="flex-shrink-0 rounded-lg bg-gray-100 p-2 text-gray-600">
          {INSIGHT_TYPE_ICONS[insight.type] ?? <BarChart3 className="h-5 w-5" />}
        </div>
        <div className="min-w-0 flex-1">
          <div className="flex items-center gap-2">
            <h3 className="font-semibold text-gray-900">{insight.title}</h3>
            <Badge variant={SEVERITY_BADGE[insight.severity] ?? "default"}>
              {SEVERITY_LABELS[insight.severity] ?? insight.severity}
            </Badge>
          </div>
          <p className="mt-1 text-sm text-gray-600">{insight.message}</p>
          <InsightDetails insight={insight} />
          <p className="mt-2 text-xs text-gray-400">
            {formatDateTime(insight.createdAt)}
          </p>
        </div>
      </div>
    </Card>
  );
}

function InsightDetails({ insight }: { insight: InsightResponse }) {
  const { type, data } = insight;

  if (type === "SPENDING_TREND") {
    const d = data as unknown as SpendingTrendData;
    return (
      <div className="mt-2 flex items-center gap-4 text-sm">
        <span className="text-gray-500">
          Mês anterior: {formatCurrency(d.previousMonth)}
        </span>
        <span className="text-gray-500">
          Mês atual: {formatCurrency(d.currentMonth)}
        </span>
        <span
          className={`flex items-center gap-1 font-medium ${
            d.direction === "up" ? "text-red-600" : "text-green-600"
          }`}
        >
          {d.direction === "up" ? (
            <TrendingUp className="h-3 w-3" />
          ) : (
            <TrendingDown className="h-3 w-3" />
          )}
          {Math.abs(d.changePercent)}%
        </span>
      </div>
    );
  }

  if (type === "MONTHLY_COMPARISON") {
    const d = data as unknown as MonthlyComparisonData;
    return (
      <div className="mt-2 grid grid-cols-3 gap-3 text-sm">
        <div className="rounded-lg bg-green-50 px-2 py-1.5 text-center">
          <p className="text-xs text-gray-500">Receitas</p>
          <p className="font-medium text-green-700">
            {formatCurrency(d.deposits.current)}
          </p>
          <ChangeIndicator value={d.deposits.changePercent} />
        </div>
        <div className="rounded-lg bg-red-50 px-2 py-1.5 text-center">
          <p className="text-xs text-gray-500">Despesas</p>
          <p className="font-medium text-red-700">
            {formatCurrency(d.outflows.current)}
          </p>
          <ChangeIndicator value={d.outflows.changePercent} inverted />
        </div>
        <div className="rounded-lg bg-blue-50 px-2 py-1.5 text-center">
          <p className="text-xs text-gray-500">Saldo Líquido</p>
          <p className="font-medium text-blue-700">
            {formatCurrency(d.netBalance.current)}
          </p>
          <ChangeIndicator value={d.netBalance.changePercent} />
        </div>
      </div>
    );
  }

  if (type === "ANOMALY_DETECTION") {
    const d = data as unknown as AnomalyData;
    return (
      <div className="mt-2 flex flex-wrap gap-3 text-sm text-gray-500">
        <span>Valor: <strong className="text-red-600">{formatCurrency(d.value)}</strong></span>
        <span>Média: {formatCurrency(d.average)}</span>
        <span>Desvio: {d.deviationsAbove}x acima</span>
      </div>
    );
  }

  return null;
}

function ChangeIndicator({
  value,
  inverted = false,
}: {
  value: number;
  inverted?: boolean;
}) {
  const isPositive = inverted ? value < 0 : value > 0;
  const isNegative = inverted ? value > 0 : value < 0;

  return (
    <span
      className={`text-xs font-medium ${
        isPositive
          ? "text-green-600"
          : isNegative
          ? "text-red-600"
          : "text-gray-500"
      }`}
    >
      {value > 0 ? "+" : ""}
      {value}%
    </span>
  );
}
