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
