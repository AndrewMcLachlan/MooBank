export interface TransactionCategory {
    id: number;
    description: string;
    isLivingExpense: boolean;
    parentId?: number;
}