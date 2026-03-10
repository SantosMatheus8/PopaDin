import { useQuery } from "@tanstack/react-query";
import { Download, FileText } from "lucide-react";
import { recordService } from "../../services/record";
import { Modal } from "../../components/Modal";
import { EmptyState } from "../../components/EmptyState";
import { formatDateTime, formatFileSize } from "../../lib/format";

interface ExportFilesProps {
  isOpen: boolean;
  onClose: () => void;
}

export function ExportFiles({ isOpen, onClose }: ExportFilesProps) {
  const { data: files, isLoading } = useQuery({
    queryKey: ["export-files"],
    queryFn: () => recordService.listExportFiles(),
    enabled: isOpen,
  });

  const handleDownload = async (fileName: string) => {
    const blob = await recordService.downloadExportFile(fileName);
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement("a");
    a.href = url;
    a.download = fileName;
    a.click();
    window.URL.revokeObjectURL(url);
  };

  return (
    <Modal isOpen={isOpen} onClose={onClose} title="Arquivos Exportados" className="max-w-2xl">
      {isLoading ? (
        <div className="flex h-32 items-center justify-center">
          <div className="h-6 w-6 animate-spin rounded-full border-4 border-primary-500 border-t-transparent" />
        </div>
      ) : !files || files.length === 0 ? (
        <EmptyState title="Nenhum arquivo" description="Nenhuma exportação disponível." />
      ) : (
        <div className="max-h-96 space-y-2 overflow-y-auto">
          {files.map((file) => (
            <div
              key={file.name}
              className="flex items-center justify-between rounded-lg border p-3"
            >
              <div className="flex items-center gap-3">
                <FileText className="h-5 w-5 text-red-500" />
                <div>
                  <p className="text-sm font-medium text-gray-700">{file.name}</p>
                  <p className="text-xs text-gray-400">
                    {formatFileSize(file.size)} - {formatDateTime(file.createdAt)}
                  </p>
                </div>
              </div>
              <button
                onClick={() => handleDownload(file.name)}
                className="rounded-lg p-2 text-gray-400 hover:bg-gray-100 hover:text-primary-500"
              >
                <Download className="h-4 w-4" />
              </button>
            </div>
          ))}
        </div>
      )}
    </Modal>
  );
}
