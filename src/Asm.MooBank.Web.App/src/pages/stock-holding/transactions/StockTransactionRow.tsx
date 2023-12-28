import React, { useState } from "react";
import { format } from "date-fns/format";
import { parseISO } from "date-fns/parseISO";

import { StockTransaction } from "models";
import { formatCurrency } from "@andrewmclachlan/mooapp";

export const StockTransactionRow: React.FC<StockTransactionRowProps> = (props) => {

    return (
        <tr className="clickable transaction-row">
            <td>{format(parseISO(props.transaction.transactionDate), "dd/MM/yyyy")}</td>
            <td className="description" colSpan={props.colspan}>{props.transaction.description}</td>
            <td>{props.transaction.accountHolderName}</td>
            <td>{props.transaction.quantity}</td>
            <td className="amount">{formatCurrency(props.transaction.price)}</td>
            <td className="amount">{formatCurrency(props.transaction.fees)}</td>
            <td className="amount">{formatCurrency((props.transaction.price * props.transaction.quantity) + props.transaction.fees)}</td>
        </tr>
    );
}

export interface StockTransactionRowProps {
    transaction: StockTransaction;
    colspan?: number
}