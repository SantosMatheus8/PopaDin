export interface NotificationEvent {
  userId: number;
  type: NotificationType;
  title: string;
  message: string;
  metadata?: Record<string, unknown>;
}

export type NotificationType =
  | 'BALANCE_BELOW'
  | 'BALANCE_ABOVE'
  | 'EXPORT_COMPLETED'
  | 'RECORD_CREATED';
