import { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { Plus, Pencil, Trash2, CheckCircle } from "lucide-react";
import toast from "react-hot-toast";
import { budgetService } from "../../services/budget";
import { useApiError } from "../../hooks/useApiError";
import { Card } from "../../components/Card";
import { Button } from "../../components/Button";
import { Badge } from "../../components/Badge";
import { Pagination } from "../../components/Pagination";
import { EmptyState } from "../../components/EmptyState";
import { ConfirmDialog } from "../../components/ConfirmDialog";
import { BudgetForm } from "./BudgetForm";
import { formatCurrency, formatDate } from "../../lib/format";
import type { BudgetResponse, ListBudgetsRequest } from "../../types";
import type { BudgetFormData } from "../../schemas/budget";

export default function BudgetsPage() {
  const queryClient = useQueryClient();
  const { handleError } = useApiError();

  const [params, setParams] = useState<ListBudgetsRequest>({ page: 1, itemsPerPage: 10 });
  const [formOpen, setFormOpen] = useState(false);
  const [editingBudget, setEditingBudget] = useState<BudgetResponse | null>(null);
  const [deleteTarget, setDeleteTarget] = useState<BudgetResponse | null>(null);
  const [finishTarget, setFinishTarget] = useState<BudgetResponse | null>(null);

  const { data, isLoading } = useQuery({
    queryKey: ["budgets", params],
    queryFn: () => budgetService.list(params),
  });

  const createMutation = useMutation({
    mutationFn: (data: BudgetFormData) => budgetService.create(data),
    onSuccess: () => {
      toast.success("Orçamento criado!");
      queryClient.invalidateQueries({ queryKey: ["budgets"] });
      queryClient.invalidateQueries({ queryKey: ["dashboard"] });
    },
    onError: (err) => handleError(err, "Erro ao criar orçamento"),
  });

  const updateMutation = useMutation({
    mutationFn: ({ id, data }: { id: number; data: BudgetFormData }) =>
      budgetService.update(id, data),
    onSuccess: () => {
      toast.success("Orçamento atualizado!");
      queryClient.invalidateQueries({ queryKey: ["budgets"] });
      queryClient.invalidateQueries({ queryKey: ["dashboard"] });
    },
    onError: (err) => handleError(err, "Erro ao atualizar orçamento"),
  });

  const deleteMutation = useMutation({
    mutationFn: (id: number) => budgetService.delete(id),
    onSuccess: () => {
      toast.success("Orçamento excluído!");
      queryClient.invalidateQueries({ queryKey: ["budgets"] });
      queryClient.invalidateQueries({ queryKey: ["dashboard"] });
      setDeleteTarget(null);
    },
    onError: (err) => handleError(err, "Erro ao excluir orçamento"),
  });

  const finishMutation = useMutation({
    mutationFn: (id: number) => budgetService.finish(id),
    onSuccess: () => {
      toast.success("Orçamento finalizado!");
      queryClient.invalidateQueries({ queryKey: ["budgets"] });
      queryClient.invalidateQueries({ queryKey: ["dashboard"] });
      setFinishTarget(null);
    },
    onError: (err) => handleError(err, "Erro ao finalizar orçamento"),
  });

  const handleCreate = async (data: BudgetFormData) => {
    await createMutation.mutateAsync(data);
  };

  const handleUpdate = async (data: BudgetFormData) => {
    if (!editingBudget) return;
    await updateMutation.mutateAsync({ id: editingBudget.id, data });
    setEditingBudget(null);
  };

  const budgets = data?.lines ?? [];

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold text-gray-900">Orçamentos</h1>
        <Button
          size="sm"
          icon={<Plus className="h-4 w-4" />}
          onClick={() => {
            setEditingBudget(null);
            setFormOpen(true);
          }}
        >
          Novo
        </Button>
      </div>

      <Card className="p-0">
        {isLoading ? (
          <div className="flex h-48 items-center justify-center">
            <div className="h-6 w-6 animate-spin rounded-full border-4 border-primary-500 border-t-transparent" />
          </div>
        ) : budgets.length === 0 ? (
          <div className="p-6">
            <EmptyState title="Nenhum orçamento" description="Crie seu primeiro orçamento." />
          </div>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full text-left text-sm">
              <thead>
                <tr className="border-b bg-gray-50 text-gray-500">
                  <th className="px-6 py-3 font-medium">Nome</th>
                  <th className="px-6 py-3 font-medium">Meta</th>
                  <th className="px-6 py-3 font-medium">Status</th>
                  <th className="px-6 py-3 font-medium">Criado em</th>
                  <th className="px-6 py-3 font-medium">Finalizado em</th>
                  <th className="px-6 py-3 font-medium">Ações</th>
                </tr>
              </thead>
              <tbody>
                {budgets.map((budget) => (
                  <tr key={budget.id} className="border-b last:border-0 hover:bg-gray-50">
                    <td className="px-6 py-4 font-medium text-gray-900">{budget.name}</td>
                    <td className="px-6 py-4 text-gray-600">{formatCurrency(budget.goal)}</td>
                    <td className="px-6 py-4">
                      <Badge variant={budget.finishAt ? "info" : "success"}>
                        {budget.finishAt ? "Finalizado" : "Ativo"}
                      </Badge>
                    </td>
                    <td className="px-6 py-4 text-gray-600">{formatDate(budget.createdAt)}</td>
                    <td className="px-6 py-4 text-gray-600">{formatDate(budget.finishAt)}</td>
                    <td className="px-6 py-4">
                      <div className="flex gap-1">
                        {!budget.finishAt && (
                          <button
                            onClick={() => setFinishTarget(budget)}
                            className="rounded p-1.5 text-gray-400 hover:bg-gray-100 hover:text-green-500"
                            title="Finalizar"
                          >
                            <CheckCircle className="h-4 w-4" />
                          </button>
                        )}
                        <button
                          onClick={() => {
                            setEditingBudget(budget);
                            setFormOpen(true);
                          }}
                          className="rounded p-1.5 text-gray-400 hover:bg-gray-100 hover:text-primary-500"
                        >
                          <Pencil className="h-4 w-4" />
                        </button>
                        <button
                          onClick={() => setDeleteTarget(budget)}
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

      <BudgetForm
        isOpen={formOpen}
        onClose={() => {
          setFormOpen(false);
          setEditingBudget(null);
        }}
        onSubmit={editingBudget ? handleUpdate : handleCreate}
        budget={editingBudget}
        isLoading={createMutation.isPending || updateMutation.isPending}
      />

      <ConfirmDialog
        isOpen={!!deleteTarget}
        onClose={() => setDeleteTarget(null)}
        onConfirm={() => deleteTarget && deleteMutation.mutate(deleteTarget.id)}
        title="Excluir orçamento"
        message="Tem certeza que deseja excluir este orçamento? Esta ação não pode ser desfeita."
        confirmLabel="Excluir"
        isLoading={deleteMutation.isPending}
      />

      <ConfirmDialog
        isOpen={!!finishTarget}
        onClose={() => setFinishTarget(null)}
        onConfirm={() => finishTarget && finishMutation.mutate(finishTarget.id)}
        title="Finalizar orçamento"
        message={`Deseja finalizar o orçamento "${finishTarget?.name}"? Ele será marcado como concluído.`}
        confirmLabel="Finalizar"
        isLoading={finishMutation.isPending}
      />
    </div>
  );
}
