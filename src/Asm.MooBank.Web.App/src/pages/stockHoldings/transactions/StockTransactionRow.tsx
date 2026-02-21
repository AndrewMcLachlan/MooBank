import { format } from "date-fns/format";
import { parseISO } from "date-fns/parseISO";
import React from "react";

import { formatCurrency } from "@andrewmclachlan/moo-ds";
import type { StockTransaction } from "api/types.gen";

export const StockTransactionRow: React.FC<StockTransactionRowProps> = (props) => {

    return (
        <tr className="clickable transaction-row">
            <td>{format(parseISO(props.transaction.transactionDate), "dd/MM/yyyy")}</td>
            <td className="description" colSpan={props.colspan}>{props.transaction.description}</td>
            <td>{props.transaction.accountHolderName}</td>
            <td><span className="amount">{props.transaction.quantity}</span></td>
            <td><span className="amount">{formatCurrency(props.transaction.price)}</span></td>
            <td><span className="amount">{formatCurrency(props.transaction.fees)}</span></td>
            <td><span className="amount">{formatCurrency((props.transaction.price * props.transaction.quantity) + props.transaction.fees)}</span></td>
        </tr>
    );
}

export interface StockTransactionRowProps {
    transaction: StockTransaction;
    colspan?: number
}
