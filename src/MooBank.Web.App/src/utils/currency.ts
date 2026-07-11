const SYMBOLS: Record<string, string> = {
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

export const getCurrencySymbol = (code: string | null | undefined): string => {
    if (!code) return "";
    const upper = code.toUpperCase();
    return SYMBOLS[upper] ?? `${upper} `;
};

export const formatCurrency = (amount: number | null | undefined, currencyCode: string = "AUD", decimalPlaces = 2): string => {
    const safeAmount = amount == null || Number.isNaN(amount) ? 0 : amount;
    const sign = safeAmount < 0 ? "-" : "";
    return `${sign}${getCurrencySymbol(currencyCode)}${Math.abs(safeAmount).toLocaleString(undefined, { minimumFractionDigits: decimalPlaces, maximumFractionDigits: decimalPlaces })}`;
};
