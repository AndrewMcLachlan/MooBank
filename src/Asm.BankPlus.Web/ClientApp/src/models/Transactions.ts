import { Transaction } from ".";

export interface Transactions {
    transactions: Transaction[];
    currentPage: number;
    pageSize: number;
    total: number;
 }