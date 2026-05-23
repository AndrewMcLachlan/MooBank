import classNames from "classnames";
import React from "react";

import { formatCurrency } from "@andrewmclachlan/moo-ds";
import type { StockTransaction } from "api/types.gen";
import { formatDateShort } from "utils/dateFns";

export const StockTransactionRow: React.FC<StockTransactionRowProps> = (props) => {

    const t = props.transaction;
    const isPurchase = t.transactionType === "Credit";
    const isSale = t.transactionType === "Debit";
    const directional = isPurchase || isSale;

    const absQty = Math.abs(t.quantity);
    const absTotal = absQty * t.price + t.fees;

    return (
        <tr className="clickable transaction-row">
            <td>{formatDateShort(t.transactionDate)}</td>
            <td className="description" colSpan={props.colspan}>{t.description}</td>
            <td>{t.accountHolderName}</td>
            <td>
                <span className={classNames("amount", isPurchase && "positive", isSale && "negative")}>
                    {directional && (isPurchase ? "+" : "−")}{absQty.toLocaleString()}
                </span>
            </td>
            <td><span className="amount">{formatCurrency(t.price)}</span></td>
            <td><span className="amount">{formatCurrency(t.fees)}</span></td>
            <td>
                <span className={classNames("amount", isSale && "positive", isPurchase && "negative")}>
                    {directional && (isSale ? "+" : "−")}{formatCurrency(absTotal)}
                </span>
            </td>
        </tr>
    );
}

export interface StockTransactionRowProps {
    transaction: StockTransaction;
    colspan?: number
}
