export const getBalanceString = (balance: number): string => {

    if (!balance) return "0.00CR";

    return balance.toLocaleString(undefined, {minimumFractionDigits: 2, maximumFractionDigits: 2}) + (balance < 0 ? "D" : "C") + "R";
}
