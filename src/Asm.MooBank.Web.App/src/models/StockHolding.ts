import { AccountBase, TopLevelAccount } from "./Account";

export interface StockHoldingBase extends AccountBase, TopLevelAccount {
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
    gainLoss: number;
}

export const emptyStockHolding: NewStockHolding = {
    id: "",
    name: "",
    symbol: "",
    quantity: 0,
    price: 0,
    fees: 0,
    groupId: "",
    currentBalance: 0,
    currentBalanceLocalCurrency: 0,
    currency: "",
    shareWithFamily: false,
}
