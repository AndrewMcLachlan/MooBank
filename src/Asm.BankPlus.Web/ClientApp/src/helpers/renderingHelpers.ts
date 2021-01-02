export const getBalanceString = (balance: number): string => {

    if (!balance) return "0CR";

    return balance + (balance < 0 ? "D" : "C") + "R";
}