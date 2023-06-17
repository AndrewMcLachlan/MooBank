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
    amount: number,
    month?: number,
    income: boolean,
}

export interface BudgetMonth {
    month: number;
    income: number;
    expenses: number;
    remainder: number;
}