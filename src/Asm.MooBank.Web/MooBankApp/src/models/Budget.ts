export interface Budget {
    year: number;
    incomeLines: BudgetLine[];
    expensesLines: BudgetLine[];
    months: BudgetMonth[];
}

export interface BudgetLine {
    id: string,
    name: string,
    tagId: number,
    notes?: string,
    amount: number,
    month?: number,
    type: BudgetLineType,
}

export type BudgetLineType = "income" | "expenses";

export interface BudgetMonth {
    month: number;
    income: number;
    expenses: number;
    remainder: number;
}

export interface BudgetReportByMonth {
    items: BudgetReportValueMonth[];
}

export interface  BudgetReportValue {
    budgetedAmount: number;
    actual: number;
}

export interface BudgetReportValueMonth extends BudgetReportValue {
    month: number;
}