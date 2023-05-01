export interface BudgetLine {
    id: string,
    name: string,
    tagId: number,
    amount: number,
    month?: number,
    income: boolean,
}