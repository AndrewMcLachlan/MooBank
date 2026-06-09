import classNames from "classnames";

import { getCurrencySymbol } from "utils/currency";

const calculateColourClass = (amount: number, positiveColour: boolean, negativeColour: boolean, zeroShowsAs: "positive" | "negative" | "neutral") => {

    if (amount !== 0) {
        const negative = amount < 0;
        return positiveColour && !negative ? "positive" :
            negativeColour && negative ? "negative" : "";
    }

    switch (zeroShowsAs) {
        case "positive":
            return "positive";
        case "negative":
            return "negative";
        case "neutral":
        default:
            return "";
    }
}

export const Amount: React.FC<AmountProps> = ({ amount, positiveColour = false, negativeColour = false, plus = false, minus = false, creditdebit = false, prefix = "", suffix = "", decimalPlaces = 2, zeroShowsAs = "neutral", currencyCode }) => {

    const negative = amount < 0;
    const cr_dr = creditdebit ? (negative ? "DR" : "CR") : "";
    const pl_mi = minus && negative ? "-" : plus && !negative ? "+" : "";
    const symbol = getCurrencySymbol(currencyCode);
    const colourClass = calculateColourClass(amount, positiveColour, negativeColour, zeroShowsAs);
    const className = classNames("amount", colourClass);

    return (<span className={className}>{`${prefix}${pl_mi}${symbol}${(Math.abs(amount) ?? 0).toLocaleString(undefined, { minimumFractionDigits: decimalPlaces, maximumFractionDigits: decimalPlaces })}${cr_dr}${suffix}`}</span>);
}

export interface AmountProps {
    amount: number,
    positiveColour?: boolean;
    negativeColour?: boolean;
    minus?: boolean;
    plus?: boolean;
    creditdebit?: boolean;
    prefix?: string;
    suffix?: string;
    decimalPlaces?: number;
    zeroShowsAs?: "positive" | "negative" | "neutral";
    currencyCode?: string;
}
