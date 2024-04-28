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


export interface CreateStockTransaction {
    quantity: number;
    price: number;
    description: string;
    fees: number;
    date: string;
}

export const emptyStockTransaction: CreateStockTransaction = {
    quantity: 0,
    price: 0,
    description: "",
    fees: 0,
    date: "",
};
