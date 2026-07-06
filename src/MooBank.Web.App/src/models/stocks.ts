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
