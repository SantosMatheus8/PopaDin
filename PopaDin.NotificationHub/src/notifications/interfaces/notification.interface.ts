export interface NotificationEvent {
  userId: number;
  type: NotificationType;
  title: string;
  message: string;
  metadata?: Record<string, unknown>;
}

export type NotificationType =
  | 'BALANCE_BELOW'
  | 'BUDGET_ABOVE'
  | 'EXPORT_COMPLETED'
  | 'RECORD_CREATED';
