import type { StockHolding } from "api/types.gen";

export const emptyStockHolding: Partial<StockHolding> & { price: number; fees: number } = {
    id: "",
    name: "",
    description: "",
    symbol: "",
    quantity: 0,
    price: 0,
    fees: 0,
    groupId: "",
    controller: "Manual",
    currentBalance: 0,
    currentBalanceLocalCurrency: 0,
    currency: "",
    shareWithFamily: false,
    virtualInstruments: [],
};

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
