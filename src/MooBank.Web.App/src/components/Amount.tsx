import classNames from "classnames";

export const Amount: React.FC<AmountProps> = ({ amount, positiveColour = false, negativeColour = false, plus = false, minus = false, creditdebit = false }) => {

    const negative = amount < 0;
    const cr_dr = creditdebit ? (negative ? "DR" : "CR") : "";
    const pl_mi = minus && negative ? "-" : plus && !negative ? "+" : "";
    const colourClass = positiveColour && !negative ? "text-success" : 
                        negativeColour && negative ? "text-danger" : "";
    const negativeClass = negativeColour ? negative ? "negative" : "" : "";
    const className = classNames("amount", colourClass, negativeClass);

    return (<span className={className}>{`${pl_mi}${(Math.abs(amount) ?? 0).toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 })}${cr_dr}`}</span>);
}

export interface AmountProps {
    amount: number,
    positiveColour?: boolean;
    negativeColour?: boolean;
    minus?: boolean;
    plus?: boolean;
    creditdebit?: boolean;
}
