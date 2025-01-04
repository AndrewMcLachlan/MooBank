import { Instrument, TopLevelAccount } from "../Instrument";

export interface StockHoldingBase extends Instrument, TopLevelAccount {
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
}
