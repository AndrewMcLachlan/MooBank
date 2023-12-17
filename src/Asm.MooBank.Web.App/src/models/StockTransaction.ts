import { TransactionType } from "./Transaction";

export interface StockTransaction {
    id: string;
    accountId: string;
    quantity: number;
    price: number;
    description: string;
    fees: number;
    accountHolderName?: string;
    transactionDate: string;
    transactionType: TransactionType;
}
