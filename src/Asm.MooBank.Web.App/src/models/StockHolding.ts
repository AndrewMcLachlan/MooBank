import { AccountBase } from "./Account";

export interface StockHoldingBase extends AccountBase {
    symbol: string;
    quantity: number;
}

export interface NewStockHolding extends StockHoldingBase {
    price: number;
    fees: number;
}

export interface StockHolding extends StockHoldingBase {
    currentPrice: number;
    value: number;
}

export const emptyStockHolding: NewStockHolding = {
    id: "",
    name: "",
    symbol: "",
    quantity: 0,
    price: 0,
    fees: 0,
    accountGroupId: "",
    currentBalance: 0,
    shareWithFamily: false,
}