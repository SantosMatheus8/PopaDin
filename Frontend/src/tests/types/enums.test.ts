import { describe, it, expect } from "vitest";
import {
  OperationEnum,
  FrequencyEnum,
  AlertType,
  OrderDirection,
  OperationLabels,
  FrequencyLabels,
  AlertTypeLabels,
} from "../../types/enums";

describe("OperationEnum", () => {
  it("deve ter Outflow = 0", () => {
    expect(OperationEnum.Outflow).toBe(0);
  });

  it("deve ter Deposit = 1", () => {
    expect(OperationEnum.Deposit).toBe(1);
  });
});

describe("FrequencyEnum", () => {
  it("deve ter todos os valores definidos", () => {
    expect(FrequencyEnum.Monthly).toBe(0);
    expect(FrequencyEnum.Bimonthly).toBe(1);
    expect(FrequencyEnum.Quarterly).toBe(2);
    expect(FrequencyEnum.Semiannual).toBe(3);
    expect(FrequencyEnum.Annual).toBe(4);
    expect(FrequencyEnum.OneTime).toBe(5);
  });
});

describe("AlertType", () => {
  it("deve ter BALANCE_BELOW = 0", () => {
    expect(AlertType.BALANCE_BELOW).toBe(0);
  });

  it("deve ter BALANCE_ABOVE = 1", () => {
    expect(AlertType.BALANCE_ABOVE).toBe(1);
  });
});

describe("OrderDirection", () => {
  it("deve ter ASC e DESC", () => {
    expect(OrderDirection.ASC).toBe("ASC");
    expect(OrderDirection.DESC).toBe("DESC");
  });
});

describe("OperationLabels", () => {
  it("deve ter labels para cada operação", () => {
    expect(OperationLabels[OperationEnum.Outflow]).toBe("Despesa");
    expect(OperationLabels[OperationEnum.Deposit]).toBe("Receita");
  });
});

describe("FrequencyLabels", () => {
  it("deve ter labels para cada frequência", () => {
    expect(FrequencyLabels[FrequencyEnum.Monthly]).toBe("Mensal");
    expect(FrequencyLabels[FrequencyEnum.Bimonthly]).toBe("Bimestral");
    expect(FrequencyLabels[FrequencyEnum.Quarterly]).toBe("Trimestral");
    expect(FrequencyLabels[FrequencyEnum.Semiannual]).toBe("Semestral");
    expect(FrequencyLabels[FrequencyEnum.Annual]).toBe("Anual");
    expect(FrequencyLabels[FrequencyEnum.OneTime]).toBe("Registro Único");
  });
});

describe("AlertTypeLabels", () => {
  it("deve ter labels para cada tipo de alerta", () => {
    expect(AlertTypeLabels[AlertType.BALANCE_BELOW]).toBe("Saldo abaixo de");
    expect(AlertTypeLabels[AlertType.BALANCE_ABOVE]).toBe("Saldo acima de");
  });
});
