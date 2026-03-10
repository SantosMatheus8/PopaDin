import { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { Plus, Trash2 } from "lucide-react";
import toast from "react-hot-toast";
import { alertService } from "../../services/alert";
import { useApiError } from "../../hooks/useApiError";
import { Card } from "../../components/Card";
import { Button } from "../../components/Button";
import { Badge } from "../../components/Badge";
import { EmptyState } from "../../components/EmptyState";
import { ConfirmDialog } from "../../components/ConfirmDialog";
import { AlertForm } from "./AlertForm";
import { formatCurrency } from "../../lib/format";
import { AlertTypeLabels, AlertType } from "../../types/enums";
import type { AlertResponse } from "../../types";
import type { AlertFormData } from "../../schemas/alert";

export default function AlertsPage() {
  const queryClient = useQueryClient();
  const { handleError } = useApiError();

  const [formOpen, setFormOpen] = useState(false);
  const [deleteTarget, setDeleteTarget] = useState<AlertResponse | null>(null);

  const { data: alerts, isLoading } = useQuery({
    queryKey: ["alerts"],
    queryFn: () => alertService.list(),
  });

  const createMutation = useMutation({
    mutationFn: (data: AlertFormData) => alertService.create(data),
    onSuccess: () => {
      toast.success("Alerta criado!");
      queryClient.invalidateQueries({ queryKey: ["alerts"] });
    },
    onError: (err) => handleError(err, "Erro ao criar alerta"),
  });

  const toggleMutation = useMutation({
    mutationFn: ({ id, active }: { id: string; active: boolean }) =>
      alertService.toggle(id, { active }),
    onSuccess: () => {
      toast.success("Alerta atualizado!");
      queryClient.invalidateQueries({ queryKey: ["alerts"] });
    },
    onError: (err) => handleError(err, "Erro ao atualizar alerta"),
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => alertService.delete(id),
    onSuccess: () => {
      toast.success("Alerta excluído!");
      queryClient.invalidateQueries({ queryKey: ["alerts"] });
      setDeleteTarget(null);
    },
    onError: (err) => handleError(err, "Erro ao excluir alerta"),
  });

  const handleCreate = async (data: AlertFormData) => {
    await createMutation.mutateAsync(data);
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold text-gray-900">Alertas</h1>
        <Button
          size="sm"
          icon={<Plus className="h-4 w-4" />}
          onClick={() => setFormOpen(true)}
        >
          Novo Alerta
        </Button>
      </div>

      {isLoading ? (
        <div className="flex h-48 items-center justify-center">
          <div className="h-6 w-6 animate-spin rounded-full border-4 border-primary-500 border-t-transparent" />
        </div>
      ) : !alerts || alerts.length === 0 ? (
        <Card>
          <EmptyState title="Nenhum alerta" description="Crie seu primeiro alerta." />
        </Card>
      ) : (
        <div className="grid grid-cols-1 gap-4 md:grid-cols-2 lg:grid-cols-3">
          {alerts.map((alert) => (
            <Card key={alert.id}>
              <div className="flex items-start justify-between">
                <div className="space-y-2">
                  <Badge variant={alert.active ? "success" : "default"}>
                    {alert.active ? "Ativo" : "Inativo"}
                  </Badge>
                  <p className="text-sm font-medium text-gray-900">
                    {AlertTypeLabels[alert.type as AlertType] || alert.type}
                  </p>
                  <p className="text-lg font-bold text-primary-700">
                    {formatCurrency(alert.threshold)}
                  </p>
                  <p className="text-xs text-gray-400">Canal: {alert.channel}</p>
                </div>
                <div className="flex gap-1">
                  <button
                    onClick={() =>
                      toggleMutation.mutate({ id: alert.id, active: !alert.active })
                    }
                    className={`rounded-full px-3 py-1 text-xs font-medium transition-colors ${
                      alert.active
                        ? "bg-gray-100 text-gray-600 hover:bg-gray-200"
                        : "bg-green-100 text-green-700 hover:bg-green-200"
                    }`}
                  >
                    {alert.active ? "Desativar" : "Ativar"}
                  </button>
                  <button
                    onClick={() => setDeleteTarget(alert)}
                    className="rounded p-1.5 text-gray-400 hover:bg-gray-100 hover:text-red-500"
                  >
                    <Trash2 className="h-4 w-4" />
                  </button>
                </div>
              </div>
            </Card>
          ))}
        </div>
      )}

      <AlertForm
        isOpen={formOpen}
        onClose={() => setFormOpen(false)}
        onSubmit={handleCreate}
        isLoading={createMutation.isPending}
      />

      <ConfirmDialog
        isOpen={!!deleteTarget}
        onClose={() => setDeleteTarget(null)}
        onConfirm={() => deleteTarget && deleteMutation.mutate(deleteTarget.id)}
        title="Excluir alerta"
        message="Tem certeza que deseja excluir este alerta?"
        confirmLabel="Excluir"
        isLoading={deleteMutation.isPending}
      />
    </div>
  );
}
