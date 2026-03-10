export enum OperationEnum {
  Outflow = 0,
  Deposit = 1,
}

export enum FrequencyEnum {
  Monthly = 0,
  Bimonthly = 1,
  Quarterly = 2,
  Semiannual = 3,
  Annual = 4,
  OneTime = 5,
}

export enum AlertType {
  BALANCE_BELOW = 0,
  BUDGET_ABOVE = 1,
}

export enum OrderDirection {
  ASC = "ASC",
  DESC = "DESC",
}

export const OperationLabels: Record<OperationEnum, string> = {
  [OperationEnum.Outflow]: "Despesa",
  [OperationEnum.Deposit]: "Receita",
};

export const FrequencyLabels: Record<FrequencyEnum, string> = {
  [FrequencyEnum.Monthly]: "Mensal",
  [FrequencyEnum.Bimonthly]: "Bimestral",
  [FrequencyEnum.Quarterly]: "Trimestral",
  [FrequencyEnum.Semiannual]: "Semestral",
  [FrequencyEnum.Annual]: "Anual",
  [FrequencyEnum.OneTime]: "Registro Único",
};

export const AlertTypeLabels: Record<AlertType, string> = {
  [AlertType.BALANCE_BELOW]: "Saldo abaixo de",
  [AlertType.BUDGET_ABOVE]: "Orçamento acima de",
};
