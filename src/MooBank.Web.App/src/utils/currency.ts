export const currencySymbols: Record<string, string> = {
    AUD: "$",
    USD: "$",
    NZD: "$",
    CAD: "$",
    HKD: "$",
    SGD: "$",
    GBP: "£",
    EUR: "€",
    JPY: "¥",
    CNY: "¥",
    INR: "₹",
    KRW: "₩",
    CHF: "CHF ",
};

/**
 * Step for monetary inputs. Amounts are persisted as decimal(12, 4), so inputs must accept
 * 4 decimal places — a coarser step makes the browser reject finer values on form submit
 * (stepMismatch), which is what blocked 4dp share prices.
 */
export const amountStep = 0.0001;

export const getCurrencySymbol = (code: string | null | undefined): string => {
    if (!code) return "";
    const upper = code.toUpperCase();
    return currencySymbols[upper] ?? `${upper} `;
};

export const formatCurrency = (amount: number | null | undefined, currencyCode: string = "AUD", decimalPlaces = 2): string => {
    const safeAmount = amount == null || Number.isNaN(amount) ? 0 : amount;
    const sign = safeAmount < 0 ? "-" : "";
    return `${sign}${getCurrencySymbol(currencyCode)}${Math.abs(safeAmount).toLocaleString(undefined, { minimumFractionDigits: decimalPlaces, maximumFractionDigits: decimalPlaces })}`;
};
