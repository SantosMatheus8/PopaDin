import {
  createContext,
  useCallback,
  useEffect,
  useRef,
  useState,
} from "react";
import { io, Socket } from "socket.io-client";
import type { NotificationResponse } from "../types";
import { useAuth } from "../hooks/useAuth";
import { notificationService } from "../services/notification";
import toast from "react-hot-toast";

interface NotificationContextData {
  unreadCount: number;
  notifications: NotificationResponse[];
  isLoading: boolean;
  loadNotifications: (page?: number) => Promise<void>;
  markAsRead: (id: string) => Promise<void>;
  markAllAsRead: () => Promise<void>;
  clearAll: () => Promise<void>;
  hasMore: boolean;
  currentPage: number;
}

export const NotificationContext = createContext<NotificationContextData>(
  {} as NotificationContextData
);

const NOTIFICATION_URL =
  import.meta.env.VITE_NOTIFICATION_URL || "http://localhost:3001";

export function NotificationProvider({
  children,
}: {
  children: React.ReactNode;
}) {
  const { isAuthenticated } = useAuth();
  const [unreadCount, setUnreadCount] = useState(0);
  const [notifications, setNotifications] = useState<NotificationResponse[]>(
    []
  );
  const [isLoading, setIsLoading] = useState(false);
  const [hasMore, setHasMore] = useState(true);
  const [currentPage, setCurrentPage] = useState(1);
  const socketRef = useRef<Socket | null>(null);

  const loadNotifications = useCallback(async (page = 1) => {
    setIsLoading(true);
    try {
      const result = await notificationService.list(page, 10);
      if (page === 1) {
        setNotifications(result.data);
      } else {
        setNotifications((prev) => [...prev, ...result.data]);
      }
      setCurrentPage(page);
      setHasMore(result.data.length === 10 && page * 10 < result.total);
    } catch {
      // silently fail
    } finally {
      setIsLoading(false);
    }
  }, []);

  const markAsRead = useCallback(
    async (id: string) => {
      try {
        await notificationService.markAsRead(id);
        setNotifications((prev) =>
          prev.map((n) =>
            n._id === id ? { ...n, read: true, readAt: new Date().toISOString() } : n
          )
        );
        setUnreadCount((prev) => Math.max(0, prev - 1));
      } catch {
        // silently fail
      }
    },
    []
  );

  const markAllAsRead = useCallback(async () => {
    try {
      await notificationService.markAllAsRead();
      setNotifications((prev) =>
        prev.map((n) => ({ ...n, read: true, readAt: new Date().toISOString() }))
      );
      setUnreadCount(0);
    } catch {
      // silently fail
    }
  }, []);

  const clearAll = useCallback(async () => {
    try {
      await notificationService.deleteAll();
      setNotifications([]);
      setUnreadCount(0);
      setHasMore(false);
      setCurrentPage(1);
    } catch {
      // silently fail
    }
  }, []);

  useEffect(() => {
    if (!isAuthenticated) {
      if (socketRef.current) {
        socketRef.current.disconnect();
        socketRef.current = null;
      }
      setNotifications([]);
      setUnreadCount(0);
      return;
    }

    const token = localStorage.getItem("access_token");
    if (!token) return;

    const socket = io(`${NOTIFICATION_URL}/notifications`, {
      auth: { token },
      transports: ["websocket", "polling"],
    });

    socket.on("connect", () => {
      console.log("NotificationHub WebSocket conectado");
    });

    socket.on("notification:count", (data: { count: number }) => {
      setUnreadCount(data.count);
    });

    socket.on("notification:new", (notification: NotificationResponse) => {
      setNotifications((prev) => [notification, ...prev]);
      toast(notification.title, {
        icon: "🔔",
        duration: 4000,
      });
    });

    socket.on("disconnect", () => {
      console.log("NotificationHub WebSocket desconectado");
    });

    socketRef.current = socket;

    loadNotifications(1);

    return () => {
      socket.disconnect();
      socketRef.current = null;
    };
  }, [isAuthenticated, loadNotifications]);

  return (
    <NotificationContext.Provider
      value={{
        unreadCount,
        notifications,
        isLoading,
        loadNotifications,
        markAsRead,
        markAllAsRead,
        clearAll,
        hasMore,
        currentPage,
      }}
    >
      {children}
    </NotificationContext.Provider>
  );
}
