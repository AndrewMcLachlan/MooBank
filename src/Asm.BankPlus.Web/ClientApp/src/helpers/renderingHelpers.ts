export const getBalanceString = (balance: number): string => {
    return balance + (balance < 0 ? "D" : "C") + "R";
}