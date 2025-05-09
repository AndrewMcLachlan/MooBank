import classNames from "classnames";

export const Amount: React.FC<AmountProps> = ({ amount, colour = false, plusminus = false, creditdebit = false }) => {

    const negative = amount < 0;
    const cr_dr = creditdebit ? (negative ? "D" : "C") : "";
    const pl_mi = plusminus ? (negative ? "-" : "+") : "";
    const colourClass = !!colour ? negative ? "text-danger" : "text-success" : "";
    const negativeClass = !!colour ? negative ? "negative" : "" : "";
    const className = classNames("amount", colourClass, negativeClass);

    return (<span className={className}>{`${pl_mi}${(amount ?? 0).toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 })}${cr_dr}`}</span>);
}

export interface AmountProps {
    amount: number,
    colour?: boolean;
    plusminus?: boolean;
    creditdebit?: boolean;
}
