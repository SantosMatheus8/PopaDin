export type InsightType = 'SPENDING_TREND' | 'BALANCE_FORECAST' | 'MONTHLY_COMPARISON' | 'ANOMALY_DETECTION';
export type InsightSeverity = 'info' | 'warning' | 'critical';

export interface InsightData {
  [key: string]: unknown;
}

export interface Insight {
  userId: number;
  type: InsightType;
  title: string;
  message: string;
  severity: InsightSeverity;
  data: InsightData;
  period: {
    start: Date;
    end: Date;
  };
  expiresAt: Date;
}

export interface InsightProcessor {
  process(userId: number): Promise<Insight[]>;
}

export function toDecimalValue(value: unknown): number {
  if (value === null || value === undefined) return 0;
  if (typeof value === 'number') return value;
  if (typeof value === 'object' && value !== null && 'toString' in value) {
    return parseFloat((value as { toString(): string }).toString());
  }
  return parseFloat(String(value));
}

export function getMonthRange(monthsAgo: number): { start: Date; end: Date } {
  const now = new Date();
  const start = new Date(now.getFullYear(), now.getMonth() - monthsAgo, 1);
  const end = new Date(now.getFullYear(), now.getMonth() - monthsAgo + 1, 0, 23, 59, 59, 999);
  return { start, end };
}

export function getEndOfNextMonth(referenceDate: Date): Date {
  return new Date(referenceDate.getFullYear(), referenceDate.getMonth() + 2, 0, 23, 59, 59, 999);
}

const FREQUENCY_MONTHS: Record<number, number> = {
  0: 1,   // Monthly
  1: 2,   // Bimonthly
  2: 3,   // Quarterly
  3: 6,   // Semiannual
  4: 12,  // Annual
};

export function getFrequencyMonths(frequency: number): number {
  return FREQUENCY_MONTHS[frequency] ?? 0;
}

/**
 * Projects all occurrence dates for a recurring record from its base date up to periodEnd.
 * Mirrors RecurrenceHelper.ProjectOccurrences from the C# backend.
 */
export function projectOccurrences(
  baseDate: Date,
  frequency: number,
  periodEnd: Date,
  recurrenceEndDate?: Date | null,
): Date[] {
  const interval = FREQUENCY_MONTHS[frequency];
  if (!interval) return [];

  const endLimit =
    recurrenceEndDate && recurrenceEndDate < periodEnd
      ? recurrenceEndDate
      : periodEnd;

  const occurrences: Date[] = [];
  let current = new Date(baseDate);

  while (current <= endLimit) {
    occurrences.push(new Date(current));
    current = new Date(current.getFullYear(), current.getMonth() + interval, current.getDate());
  }

  return occurrences;
}
