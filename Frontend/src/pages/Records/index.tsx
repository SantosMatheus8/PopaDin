import { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { Plus, Download, FolderOpen, Pencil, Trash2 } from "lucide-react";
import toast from "react-hot-toast";
import { recordService } from "../../services/record";
import { useApiError } from "../../hooks/useApiError";
import { Card } from "../../components/Card";
import { Button } from "../../components/Button";
import { Badge } from "../../components/Badge";
import { Pagination } from "../../components/Pagination";
import { EmptyState } from "../../components/EmptyState";
import { ConfirmDialog } from "../../components/ConfirmDialog";
import { RecordForm } from "./RecordForm";
import { ExportModal } from "./ExportModal";
import { ExportFiles } from "./ExportFiles";
import { formatCurrency, formatDate } from "../../lib/format";
import { OperationLabels, FrequencyLabels, OperationEnum } from "../../types/enums";
import type { RecordResponse, ListRecordsRequest } from "../../types";
import type { RecordFormData } from "../../schemas/record";
import type { ExportRecordsFormData } from "../../schemas/record";

export default function RecordsPage() {
  const queryClient = useQueryClient();
  const { handleError } = useApiError();

  const [params, setParams] = useState<ListRecordsRequest>({ page: 1, itemsPerPage: 10 });
  const [formOpen, setFormOpen] = useState(false);
  const [editingRecord, setEditingRecord] = useState<RecordResponse | null>(null);
  const [deleteTarget, setDeleteTarget] = useState<RecordResponse | null>(null);
  const [exportOpen, setExportOpen] = useState(false);
  const [filesOpen, setFilesOpen] = useState(false);

  const { data, isLoading } = useQuery({
    queryKey: ["records", params],
    queryFn: () => recordService.list(params),
  });

  const createMutation = useMutation({
    mutationFn: (data: RecordFormData) =>
      recordService.create({ ...data, tagIds: data.tagIds ?? [], installments: data.installments }),
    onSuccess: () => {
      toast.success("Registro criado!");
      queryClient.invalidateQueries({ queryKey: ["records"] });
      queryClient.invalidateQueries({ queryKey: ["dashboard"] });
    },
    onError: (err) => handleError(err, "Erro ao criar registro"),
  });

  const updateMutation = useMutation({
    mutationFn: ({ id, data }: { id: string; data: RecordFormData }) =>
      recordService.update(id, { ...data, tagIds: data.tagIds ?? [], installments: data.installments }),
    onSuccess: () => {
      toast.success("Registro atualizado!");
      queryClient.invalidateQueries({ queryKey: ["records"] });
      queryClient.invalidateQueries({ queryKey: ["dashboard"] });
    },
    onError: (err) => handleError(err, "Erro ao atualizar registro"),
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => recordService.delete(id),
    onSuccess: () => {
      toast.success("Registro excluído!");
      queryClient.invalidateQueries({ queryKey: ["records"] });
      queryClient.invalidateQueries({ queryKey: ["dashboard"] });
      setDeleteTarget(null);
    },
    onError: (err) => handleError(err, "Erro ao excluir registro"),
  });

  const exportMutation = useMutation({
    mutationFn: (data: ExportRecordsFormData) => recordService.exportRecords(data),
    onSuccess: () => {
      toast.success("Exportação solicitada! O arquivo estará disponível em breve.");
    },
    onError: (err) => handleError(err, "Erro ao solicitar exportação"),
  });

  const handleCreate = async (data: RecordFormData) => {
    await createMutation.mutateAsync(data);
  };

  const handleUpdate = async (data: RecordFormData) => {
    if (!editingRecord) return;
    await updateMutation.mutateAsync({ id: editingRecord.id, data });
    setEditingRecord(null);
  };

  const handleEdit = (record: RecordResponse) => {
    setEditingRecord(record);
    setFormOpen(true);
  };

  const records = data?.lines ?? [];

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold text-gray-900">Registros</h1>
        <div className="flex gap-2">
          <Button
            variant="outline"
            size="sm"
            icon={<FolderOpen className="h-4 w-4" />}
            onClick={() => setFilesOpen(true)}
          >
            Arquivos
          </Button>
          <Button
            variant="outline"
            size="sm"
            icon={<Download className="h-4 w-4" />}
            onClick={() => setExportOpen(true)}
          >
            Exportar
          </Button>
          <Button
            size="sm"
            icon={<Plus className="h-4 w-4" />}
            onClick={() => {
              setEditingRecord(null);
              setFormOpen(true);
            }}
          >
            Novo
          </Button>
        </div>
      </div>

      <Card className="p-0">
        {isLoading ? (
          <div className="flex h-48 items-center justify-center">
            <div className="h-6 w-6 animate-spin rounded-full border-4 border-primary-500 border-t-transparent" />
          </div>
        ) : records.length === 0 ? (
          <div className="p-6">
            <EmptyState title="Nenhum registro" description="Crie seu primeiro registro." />
          </div>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full text-left text-sm">
              <thead>
                <tr className="border-b bg-gray-50 text-gray-500">
                  <th className="px-6 py-3 font-medium">Data</th>
                  <th className="px-6 py-3 font-medium">Nome</th>
                  <th className="px-6 py-3 font-medium">Tipo</th>
                  <th className="px-6 py-3 font-medium">Valor</th>
                  <th className="px-6 py-3 font-medium">Frequência</th>
                  <th className="px-6 py-3 font-medium">Tags</th>
                  <th className="px-6 py-3 font-medium">Ações</th>
                </tr>
              </thead>
              <tbody>
                {records.map((record) => (
                  <tr key={record.id} className="border-b last:border-0 hover:bg-gray-50">
                    <td className="px-6 py-4 text-gray-600">{formatDate(record.referenceDate)}</td>
                    <td className="px-6 py-4 font-medium text-gray-900">
                      {record.name || "-"}
                      {record.installmentTotal != null && record.installmentTotal > 1 && (
                        <span className="ml-2 inline-flex items-center rounded-full bg-blue-100 px-2 py-0.5 text-xs font-medium text-blue-700">
                          {record.installmentIndex}/{record.installmentTotal}
                        </span>
                      )}
                    </td>
                    <td className="px-6 py-4">
                      <Badge variant={record.operation === OperationEnum.Deposit ? "success" : "error"}>
                        {OperationLabels[record.operation]}
                      </Badge>
                    </td>
                    <td className={`px-6 py-4 font-medium ${record.operation === OperationEnum.Deposit ? "text-green-600" : "text-red-600"}`}>
                      {formatCurrency(record.value)}
                    </td>
                    <td className="px-6 py-4 text-gray-600">{FrequencyLabels[record.frequency]}</td>
                    <td className="px-6 py-4">
                      <div className="flex flex-wrap gap-1">
                        {record.tags.map((tag) => (
                          <Badge
                            key={tag.id}
                            variant="info"
                            style={tag.color ? { backgroundColor: tag.color + "20", color: tag.color } : undefined}
                          >
                            {tag.name}
                          </Badge>
                        ))}
                      </div>
                    </td>
                    <td className="px-6 py-4">
                      <div className="flex gap-1">
                        <button
                          onClick={() => handleEdit(record)}
                          className="rounded p-1.5 text-gray-400 hover:bg-gray-100 hover:text-primary-500"
                        >
                          <Pencil className="h-4 w-4" />
                        </button>
                        <button
                          onClick={() => setDeleteTarget(record)}
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

      <RecordForm
        isOpen={formOpen}
        onClose={() => {
          setFormOpen(false);
          setEditingRecord(null);
        }}
        onSubmit={editingRecord ? handleUpdate : handleCreate}
        record={editingRecord}
        isLoading={createMutation.isPending || updateMutation.isPending}
      />

      <ExportModal
        isOpen={exportOpen}
        onClose={() => setExportOpen(false)}
        onSubmit={(data) => exportMutation.mutateAsync(data)}
        isLoading={exportMutation.isPending}
      />

      <ExportFiles isOpen={filesOpen} onClose={() => setFilesOpen(false)} />

      <ConfirmDialog
        isOpen={!!deleteTarget}
        onClose={() => setDeleteTarget(null)}
        onConfirm={() => deleteTarget && deleteMutation.mutate(deleteTarget.id)}
        title="Excluir registro"
        message={
          deleteTarget?.installmentTotal != null && deleteTarget.installmentTotal > 1
            ? `Este registro faz parte de um parcelamento (${deleteTarget.installmentTotal}x). Todas as parcelas serão excluídas. Esta ação não pode ser desfeita.`
            : "Tem certeza que deseja excluir este registro? Esta ação não pode ser desfeita."
        }
        confirmLabel="Excluir"
        isLoading={deleteMutation.isPending}
      />
    </div>
  );
}
