import { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { Plus, Pencil, Trash2, CheckCircle } from "lucide-react";
import toast from "react-hot-toast";
import { goalService } from "../../services/goal";
import { useApiError } from "../../hooks/useApiError";
import { Card } from "../../components/Card";
import { Button } from "../../components/Button";
import { Badge } from "../../components/Badge";
import { Pagination } from "../../components/Pagination";
import { EmptyState } from "../../components/EmptyState";
import { ConfirmDialog } from "../../components/ConfirmDialog";
import { GoalForm } from "./GoalForm";
import { formatCurrency, formatDate } from "../../lib/format";
import type { GoalResponse, ListGoalsRequest } from "../../types";
import type { GoalFormData } from "../../schemas/goal";

export default function GoalsPage() {
  const queryClient = useQueryClient();
  const { handleError } = useApiError();

  const [params, setParams] = useState<ListGoalsRequest>({ page: 1, itemsPerPage: 10 });
  const [formOpen, setFormOpen] = useState(false);
  const [editingGoal, setEditingGoal] = useState<GoalResponse | null>(null);
  const [deleteTarget, setDeleteTarget] = useState<GoalResponse | null>(null);
  const [finishTarget, setFinishTarget] = useState<GoalResponse | null>(null);

  const { data, isLoading } = useQuery({
    queryKey: ["goals", params],
    queryFn: () => goalService.list(params),
  });

  const createMutation = useMutation({
    mutationFn: (data: GoalFormData) => goalService.create(data),
    onSuccess: () => {
      toast.success("Meta criada!");
      queryClient.invalidateQueries({ queryKey: ["goals"] });
      queryClient.invalidateQueries({ queryKey: ["dashboard"] });
    },
    onError: (err) => handleError(err, "Erro ao criar meta"),
  });

  const updateMutation = useMutation({
    mutationFn: ({ id, data }: { id: number; data: GoalFormData }) =>
      goalService.update(id, data),
    onSuccess: () => {
      toast.success("Meta atualizada!");
      queryClient.invalidateQueries({ queryKey: ["goals"] });
      queryClient.invalidateQueries({ queryKey: ["dashboard"] });
    },
    onError: (err) => handleError(err, "Erro ao atualizar meta"),
  });

  const deleteMutation = useMutation({
    mutationFn: (id: number) => goalService.delete(id),
    onSuccess: () => {
      toast.success("Meta excluída!");
      queryClient.invalidateQueries({ queryKey: ["goals"] });
      queryClient.invalidateQueries({ queryKey: ["dashboard"] });
      setDeleteTarget(null);
    },
    onError: (err) => handleError(err, "Erro ao excluir meta"),
  });

  const finishMutation = useMutation({
    mutationFn: (id: number) => goalService.finish(id),
    onSuccess: () => {
      toast.success("Meta batida! Parabéns!");
      queryClient.invalidateQueries({ queryKey: ["goals"] });
      queryClient.invalidateQueries({ queryKey: ["dashboard"] });
      setFinishTarget(null);
    },
    onError: (err) => handleError(err, "Erro ao finalizar meta"),
  });

  const handleCreate = async (data: GoalFormData) => {
    await createMutation.mutateAsync(data);
  };

  const handleUpdate = async (data: GoalFormData) => {
    if (!editingGoal) return;
    await updateMutation.mutateAsync({ id: editingGoal.id, data });
    setEditingGoal(null);
  };

  const goals = data?.lines ?? [];

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold text-gray-900">Metas</h1>
        <Button
          size="sm"
          icon={<Plus className="h-4 w-4" />}
          onClick={() => {
            setEditingGoal(null);
            setFormOpen(true);
          }}
        >
          Nova
        </Button>
      </div>

      <Card className="p-0">
        {isLoading ? (
          <div className="flex h-48 items-center justify-center">
            <div className="h-6 w-6 animate-spin rounded-full border-4 border-primary-500 border-t-transparent" />
          </div>
        ) : goals.length === 0 ? (
          <div className="p-6">
            <EmptyState title="Nenhuma meta" description="Crie sua primeira meta de economia." />
          </div>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full text-left text-sm">
              <thead>
                <tr className="border-b bg-gray-50 text-gray-500">
                  <th className="px-6 py-3 font-medium">Nome</th>
                  <th className="px-6 py-3 font-medium">Valor da Meta</th>
                  <th className="px-6 py-3 font-medium">Data Limite</th>
                  <th className="px-6 py-3 font-medium">Status</th>
                  <th className="px-6 py-3 font-medium">Criado em</th>
                  <th className="px-6 py-3 font-medium">Batida em</th>
                  <th className="px-6 py-3 font-medium">Ações</th>
                </tr>
              </thead>
              <tbody>
                {goals.map((goal) => (
                  <tr key={goal.id} className="border-b last:border-0 hover:bg-gray-50">
                    <td className="px-6 py-4 font-medium text-gray-900">{goal.name}</td>
                    <td className="px-6 py-4 text-gray-600">{formatCurrency(goal.targetAmount)}</td>
                    <td className="px-6 py-4 text-gray-600">{formatDate(goal.deadline)}</td>
                    <td className="px-6 py-4">
                      <Badge variant={goal.finishAt ? "success" : "warning"}>
                        {goal.finishAt ? "Batida" : "Em andamento"}
                      </Badge>
                    </td>
                    <td className="px-6 py-4 text-gray-600">{formatDate(goal.createdAt)}</td>
                    <td className="px-6 py-4 text-gray-600">{formatDate(goal.finishAt)}</td>
                    <td className="px-6 py-4">
                      <div className="flex gap-1">
                        {!goal.finishAt && (
                          <button
                            onClick={() => setFinishTarget(goal)}
                            className="rounded p-1.5 text-gray-400 hover:bg-gray-100 hover:text-green-500"
                            title="Marcar como batida"
                          >
                            <CheckCircle className="h-4 w-4" />
                          </button>
                        )}
                        <button
                          onClick={() => {
                            setEditingGoal(goal);
                            setFormOpen(true);
                          }}
                          className="rounded p-1.5 text-gray-400 hover:bg-gray-100 hover:text-primary-500"
                        >
                          <Pencil className="h-4 w-4" />
                        </button>
                        <button
                          onClick={() => setDeleteTarget(goal)}
                          className="rounded p-1.5 text-gray-400 hover:bg-gray-100 hover:text-red-500"
                        >
                          <Trash2 className="h-4 w-4" />
                        </button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}

        {data && (
          <div className="px-6 pb-4">
            <Pagination
              page={data.page}
              totalPages={data.totalPages}
              onPageChange={(p) => setParams((prev) => ({ ...prev, page: p }))}
            />
          </div>
        )}
      </Card>

      <GoalForm
        isOpen={formOpen}
        onClose={() => {
          setFormOpen(false);
          setEditingGoal(null);
        }}
        onSubmit={editingGoal ? handleUpdate : handleCreate}
        goal={editingGoal}
        isLoading={createMutation.isPending || updateMutation.isPending}
      />

      <ConfirmDialog
        isOpen={!!deleteTarget}
        onClose={() => setDeleteTarget(null)}
        onConfirm={() => deleteTarget && deleteMutation.mutate(deleteTarget.id)}
        title="Excluir meta"
        message="Tem certeza que deseja excluir esta meta? Esta ação não pode ser desfeita."
        confirmLabel="Excluir"
        isLoading={deleteMutation.isPending}
      />

      <ConfirmDialog
        isOpen={!!finishTarget}
        onClose={() => setFinishTarget(null)}
        onConfirm={() => finishTarget && finishMutation.mutate(finishTarget.id)}
        title="Marcar meta como batida"
        message={`Deseja marcar a meta "${finishTarget?.name}" como batida? Parabéns por alcançar seu objetivo!`}
        confirmLabel="Confirmar"
        isLoading={finishMutation.isPending}
      />
    </div>
  );
}
