export const getBalanceString = (balance: number): string => {

    if (!balance) return "0CR";

    return balance.toFixed(2) + (balance < 0 ? "D" : "C") + "R";
}

export const formatCurrency = (amount: number): string => amount.toLocaleString("en-AU", { minimumFractionDigits: 2, maximumFractionDigits: 2 })