import { format } from "date-fns/format";
import { parseISO } from "date-fns/parseISO";
import React from "react";

import { formatCurrency } from "@andrewmclachlan/mooapp";
import { Transaction } from "models";
import { TransactionTagPanel } from "./TransactionTagPanel";

export const TransactionRow: React.FC<TransactionRowProps> = (props) => {

    return (
        <>
            <tr className="clickable transaction-row" onClick={() => props.onClick(props.transaction)} title={props.transaction.notes}>
                <td>{format(parseISO(props.transaction.transactionTime), "dd/MM/yy")}</td>
                <td className="description" colSpan={props.colspan}>{props.transaction.description}</td>
                <td className="d-none d-md-table-cell">{props.transaction.location}</td>
                <td className="d-none d-md-table-cell">{props.transaction.accountHolderName}</td>
                <td className="amount">{formatCurrency(props.transaction.amount)}</td>
                <TransactionTagPanel as="td" transaction={props.transaction} />
            </tr>
        </>
    );
}

export interface TransactionRowProps {
    transaction: Transaction;
    colspan?: number;
    onClick: (transaction: Transaction) => void;
}
