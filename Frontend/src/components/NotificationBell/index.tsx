import { useState, useRef, useEffect } from "react";
import { Bell, Check, CheckCheck, Trash2 } from "lucide-react";
import { cn } from "../../lib/utils";
import { useNotifications } from "../../hooks/useNotifications";
import { formatDateTime } from "../../lib/format";

const typeLabels: Record<string, string> = {
  BALANCE_BELOW: "Saldo Baixo",
  BUDGET_ABOVE: "Orçamento Excedido",
  EXPORT_COMPLETED: "Exportação Concluída",
  RECORD_CREATED: "Record Criado",
};

const typeColors: Record<string, string> = {
  BALANCE_BELOW: "bg-red-500",
  BUDGET_ABOVE: "bg-amber-500",
  EXPORT_COMPLETED: "bg-green-500",
  RECORD_CREATED: "bg-blue-500",
};

export function NotificationBell() {
  const [isOpen, setIsOpen] = useState(false);
  const panelRef = useRef<HTMLDivElement>(null);
  const {
    unreadCount,
    notifications,
    isLoading,
    loadNotifications,
    markAsRead,
    markAllAsRead,
    clearAll,
    hasMore,
    currentPage,
  } = useNotifications();

  useEffect(() => {
    function handleClickOutside(event: MouseEvent) {
      if (
        panelRef.current &&
        !panelRef.current.contains(event.target as Node)
      ) {
        setIsOpen(false);
      }
    }

    if (isOpen) {
      document.addEventListener("mousedown", handleClickOutside);
    }
    return () => document.removeEventListener("mousedown", handleClickOutside);
  }, [isOpen]);

  return (
    <div className="relative" ref={panelRef}>
      <button
        onClick={() => setIsOpen(!isOpen)}
        className="relative flex items-center gap-2 rounded-lg px-3 py-2.5 text-sm font-medium text-gray-300 transition-colors hover:bg-secondary-700 hover:text-white"
      >
        <Bell className="h-5 w-5" />
        Notificações
        {unreadCount > 0 && (
          <span className="absolute -top-1 left-7 flex h-5 min-w-5 items-center justify-center rounded-full bg-red-500 px-1 text-[10px] font-bold text-white">
            {unreadCount > 99 ? "99+" : unreadCount}
          </span>
        )}
      </button>

      {isOpen && (
        <div className="absolute bottom-full left-0 z-50 mb-2 w-80 overflow-hidden rounded-lg border border-gray-200 bg-white shadow-xl">
          <div className="flex items-center justify-between border-b border-gray-100 px-4 py-3">
            <h3 className="text-sm font-semibold text-gray-900">
              Notificações
            </h3>
            <div className="flex items-center gap-2">
              {unreadCount > 0 && (
                <button
                  onClick={markAllAsRead}
                  className="flex items-center gap-1 text-xs text-primary-600 hover:text-primary-700"
                >
                  <CheckCheck className="h-3.5 w-3.5" />
                  Marcar lidas
                </button>
              )}
              {notifications.length > 0 && (
                <button
                  onClick={clearAll}
                  className="flex items-center gap-1 rounded px-1.5 py-0.5 text-xs text-red-500 cursor-pointer transition-all hover:text-red-600 hover:bg-red-50 active:scale-95"
                >
                  <Trash2 className="h-3.5 w-3.5" />
                  Limpar
                </button>
              )}
            </div>
          </div>

          <div className="max-h-96 overflow-y-auto">
            {notifications.length === 0 && !isLoading ? (
              <div className="px-4 py-8 text-center text-sm text-gray-400">
                Nenhuma notificação
              </div>
            ) : (
              <>
                {notifications.map((notification) => (
                  <div
                    key={notification._id}
                    className={cn(
                      "flex gap-3 border-b border-gray-50 px-4 py-3 transition-colors",
                      !notification.read
                        ? "bg-blue-50/50 hover:bg-blue-50"
                        : "hover:bg-gray-50"
                    )}
                  >
                    <div
                      className={cn(
                        "mt-1 h-2 w-2 flex-shrink-0 rounded-full",
                        typeColors[notification.type] ?? "bg-gray-400"
                      )}
                    />
                    <div className="min-w-0 flex-1">
                      <div className="flex items-start justify-between gap-2">
                        <div>
                          <span className="text-[10px] font-medium uppercase tracking-wider text-gray-400">
                            {typeLabels[notification.type] ?? notification.type}
                          </span>
                          <p className="text-sm font-medium text-gray-900">
                            {notification.title}
                          </p>
                        </div>
                        {!notification.read && (
                          <button
                            onClick={() => markAsRead(notification._id)}
                            className="flex-shrink-0 rounded p-1 text-gray-400 hover:bg-gray-100 hover:text-gray-600"
                            title="Marcar como lida"
                          >
                            <Check className="h-3.5 w-3.5" />
                          </button>
                        )}
                      </div>
                      <p className="mt-0.5 text-xs text-gray-500">
                        {notification.message}
                      </p>
                      <p className="mt-1 text-[10px] text-gray-400">
                        {formatDateTime(notification.createdAt)}
                      </p>
                    </div>
                  </div>
                ))}

                {hasMore && (
                  <button
                    onClick={() => loadNotifications(currentPage + 1)}
                    disabled={isLoading}
                    className="w-full px-4 py-2.5 text-center text-xs font-medium text-primary-600 hover:bg-gray-50 disabled:opacity-50"
                  >
                    {isLoading ? "Carregando..." : "Ver mais"}
                  </button>
                )}
              </>
            )}
          </div>
        </div>
      )}
    </div>
  );
}
