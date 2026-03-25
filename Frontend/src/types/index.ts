import { FrequencyEnum, OperationEnum, OrderDirection } from "./enums";

// --- API Error ---
export interface ApiError {
  statusCode: number;
  message: string;
  detail?: string;
  title?: string;
  errors?: Record<string, string[]>;
}

// --- Pagination ---
export interface PaginatedResult<T> {
  lines: T[];
  page: number;
  pageSize: number;
  totalItems: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export interface PaginationParams {
  page?: number;
  itemsPerPage?: number;
  orderDirection?: OrderDirection;
}

// --- User ---
export interface UserResponse {
  id: number;
  name: string;
  email: string;
  balance: number;
  profilePictureUrl: string | null;
  createdAt: string;
  updatedAt: string;
}

export interface CreateUserRequest {
  name: string;
  email: string;
  password: string;
}

export interface UpdateUserRequest {
  name: string;
  password?: string;
}

export interface ListUsersRequest extends PaginationParams {
  id?: number;
  name?: string;
  email?: string;
  balance?: number;
  orderBy?: "Id" | "Name" | "Email" | "Balance";
}

// --- Auth ---
export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  access_token: string;
}

// --- Tag ---
export interface TagResponse {
  id: number | null;
  name: string;
  tagType: OperationEnum | null;
  description: string | null;
  color: string | null;
  userId: number;
  createdAt: string | null;
}

export interface CreateTagRequest {
  name: string;
  tagType?: OperationEnum | null;
  description?: string | null;
  color?: string | null;
}

export interface UpdateTagRequest {
  name: string;
  tagType?: OperationEnum | null;
  description?: string | null;
  color?: string | null;
}

export interface ListTagsRequest extends PaginationParams {
  id?: number;
  name?: string;
  tagType?: OperationEnum;
  description?: string;
  orderBy?: "Id" | "Name" | "TagType" | "CreatedAt";
}

// --- Record ---
export interface RecordTagResponse {
  id: number | null;
  name: string;
  tagType: OperationEnum | null;
  color: string | null;
}

export interface RecordResponse {
  id: string;
  name: string;
  operation: OperationEnum;
  value: number;
  frequency: FrequencyEnum;
  userId: number;
  referenceDate: string;
  createdAt: string;
  updatedAt: string;
  tags: RecordTagResponse[];
  installmentGroupId: string | null;
  installmentIndex: number | null;
  installmentTotal: number | null;
  recurrenceEndDate: string | null;
}

export interface CreateRecordRequest {
  name: string;
  operation: OperationEnum;
  value: number;
  frequency: FrequencyEnum;
  tagIds: number[];
  referenceDate?: string;
  installments?: number;
  recurrenceEndDate?: string;
}

export interface UpdateRecordRequest {
  name: string;
  operation: OperationEnum;
  value: number;
  frequency: FrequencyEnum;
  tagIds: number[];
  referenceDate?: string;
  installments?: number;
  recurrenceEndDate?: string;
}

export interface ListRecordsRequest extends PaginationParams {
  id?: number;
  operation?: OperationEnum;
  frequency?: FrequencyEnum;
  orderBy?: "Id" | "CreatedAt" | "Frequency" | "Value" | "Operation";
}

export interface ExportRecordsRequest {
  startDate: string;
  endDate: string;
}

export interface ExportFileResponse {
  name: string;
  url: string;
  size: number;
  createdAt: string | null;
}

// --- Goal (Meta) ---
export interface GoalResponse {
  id: number;
  name: string;
  targetAmount: number;
  userId: number;
  deadline: string | null;
  finishAt: string | null;
  createdAt: string;
  updatedAt: string;
}

export interface CreateGoalRequest {
  name: string;
  targetAmount: number;
  deadline?: string | null;
}

export interface UpdateGoalRequest {
  name: string;
  targetAmount: number;
  deadline?: string | null;
}

export interface ListGoalsRequest extends PaginationParams {
  id?: number;
  name?: string;
  targetAmount?: number;
  orderBy?: "Id" | "Name" | "TargetAmount" | "Deadline" | "FinishAt";
}

// --- Alert ---
export interface AlertResponse {
  id: string;
  userId: number;
  type: string;
  threshold: number;
  channel: string;
  active: boolean;
  createdAt: string;
}

export interface CreateAlertRequest {
  type: number;
  threshold: number;
}

export interface ToggleAlertRequest {
  active: boolean;
}

// --- Notification ---
export interface NotificationResponse {
  _id: string;
  userId: number;
  type: string;
  title: string;
  message: string;
  metadata: Record<string, unknown>;
  read: boolean;
  createdAt: string;
  readAt: string | null;
}

export interface NotificationListResponse {
  data: NotificationResponse[];
  total: number;
  page: number;
  limit: number;
}

// --- Dashboard ---
export interface DashboardRequest {
  startDate?: string;
  endDate?: string;
}

export interface DashboardSummaryResponse {
  totalDeposits: number;
  totalOutflows: number;
  balance: number;
  recordCount: number;
}

export interface DashboardGoalResponse {
  id: number;
  name: string;
  targetAmount: number;
  totalSaved: number;
  savedPercentage: number;
  status: "ok" | "close" | "achieved";
}

export interface DashboardSpendingByTagResponse {
  tagId: number;
  tagName: string;
  totalSpent: number;
}

export interface DashboardResponse {
  summary: DashboardSummaryResponse;
  goals: DashboardGoalResponse[];
  spendingByTag: DashboardSpendingByTagResponse[];
  latestRecords: RecordResponse[];
  topDeposits: RecordResponse[];
  topOutflows: RecordResponse[];
}
