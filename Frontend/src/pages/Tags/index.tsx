import { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { Plus, Pencil, Trash2 } from "lucide-react";
import toast from "react-hot-toast";
import { tagService } from "../../services/tag";
import { useApiError } from "../../hooks/useApiError";
import { Card } from "../../components/Card";
import { Button } from "../../components/Button";
import { Badge } from "../../components/Badge";
import { Pagination } from "../../components/Pagination";
import { EmptyState } from "../../components/EmptyState";
import { ConfirmDialog } from "../../components/ConfirmDialog";
import { TagForm } from "./TagForm";
import { formatDate } from "../../lib/format";
import { OperationLabels, OperationEnum } from "../../types/enums";
import type { TagResponse, ListTagsRequest } from "../../types";
import type { TagFormData } from "../../schemas/tag";

export default function TagsPage() {
  const queryClient = useQueryClient();
  const { handleError } = useApiError();

  const [params, setParams] = useState<ListTagsRequest>({ page: 1, itemsPerPage: 10 });
  const [formOpen, setFormOpen] = useState(false);
  const [editingTag, setEditingTag] = useState<TagResponse | null>(null);
  const [deleteTarget, setDeleteTarget] = useState<TagResponse | null>(null);

  const { data, isLoading } = useQuery({
    queryKey: ["tags", params],
    queryFn: () => tagService.list(params),
  });

  const createMutation = useMutation({
    mutationFn: (data: TagFormData) => tagService.create(data),
    onSuccess: () => {
      toast.success("Tag criada!");
      queryClient.invalidateQueries({ queryKey: ["tags"] });
    },
    onError: (err) => handleError(err, "Erro ao criar tag"),
  });

  const updateMutation = useMutation({
    mutationFn: ({ id, data }: { id: number; data: TagFormData }) =>
      tagService.update(id, data),
    onSuccess: () => {
      toast.success("Tag atualizada!");
      queryClient.invalidateQueries({ queryKey: ["tags"] });
    },
    onError: (err) => handleError(err, "Erro ao atualizar tag"),
  });

  const deleteMutation = useMutation({
    mutationFn: (id: number) => tagService.delete(id),
    onSuccess: () => {
      toast.success("Tag excluída!");
      queryClient.invalidateQueries({ queryKey: ["tags"] });
      setDeleteTarget(null);
    },
    onError: (err) => handleError(err, "Erro ao excluir tag"),
  });

  const handleCreate = async (data: TagFormData) => {
    await createMutation.mutateAsync(data);
  };

  const handleUpdate = async (data: TagFormData) => {
    if (!editingTag?.id) return;
    await updateMutation.mutateAsync({ id: editingTag.id, data });
    setEditingTag(null);
  };

  const tags = data?.lines ?? [];

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold text-gray-900">Tags</h1>
        <Button
          size="sm"
          icon={<Plus className="h-4 w-4" />}
          onClick={() => {
            setEditingTag(null);
            setFormOpen(true);
          }}
        >
          Nova Tag
        </Button>
      </div>

      <Card className="p-0">
        {isLoading ? (
          <div className="flex h-48 items-center justify-center">
            <div className="h-6 w-6 animate-spin rounded-full border-4 border-primary-500 border-t-transparent" />
          </div>
        ) : tags.length === 0 ? (
          <div className="p-6">
            <EmptyState title="Nenhuma tag" description="Crie sua primeira tag." />
          </div>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full text-left text-sm">
              <thead>
                <tr className="border-b bg-gray-50 text-gray-500">
                  <th className="px-6 py-3 font-medium">Nome</th>
                  <th className="px-6 py-3 font-medium">Tipo</th>
                  <th className="px-6 py-3 font-medium">Descrição</th>
                  <th className="px-6 py-3 font-medium">Criada em</th>
                  <th className="px-6 py-3 font-medium">Ações</th>
                </tr>
              </thead>
              <tbody>
                {tags.map((tag) => (
                  <tr key={tag.id} className="border-b last:border-0 hover:bg-gray-50">
                    <td className="px-6 py-4 font-medium text-gray-900">{tag.name}</td>
                    <td className="px-6 py-4">
                      {tag.tagType !== null ? (
                        <Badge variant={tag.tagType === OperationEnum.Deposit ? "success" : "error"}>
                          {OperationLabels[tag.tagType]}
                        </Badge>
                      ) : (
                        <span className="text-gray-400">-</span>
                      )}
                    </td>
                    <td className="px-6 py-4 text-gray-600">{tag.description || "-"}</td>
                    <td className="px-6 py-4 text-gray-600">{formatDate(tag.createdAt)}</td>
                    <td className="px-6 py-4">
                      <div className="flex gap-1">
                        <button
                          onClick={() => {
                            setEditingTag(tag);
                            setFormOpen(true);
                          }}
                          className="rounded p-1.5 text-gray-400 hover:bg-gray-100 hover:text-primary-500"
                        >
                          <Pencil className="h-4 w-4" />
                        </button>
                        <button
                          onClick={() => setDeleteTarget(tag)}
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

      <TagForm
        isOpen={formOpen}
        onClose={() => {
          setFormOpen(false);
          setEditingTag(null);
        }}
        onSubmit={editingTag ? handleUpdate : handleCreate}
        tag={editingTag}
        isLoading={createMutation.isPending || updateMutation.isPending}
      />

      <ConfirmDialog
        isOpen={!!deleteTarget}
        onClose={() => setDeleteTarget(null)}
        onConfirm={() => deleteTarget?.id && deleteMutation.mutate(deleteTarget.id)}
        title="Excluir tag"
        message="Tem certeza que deseja excluir esta tag? Esta ação não pode ser desfeita."
        confirmLabel="Excluir"
        isLoading={deleteMutation.isPending}
      />
    </div>
  );
}
