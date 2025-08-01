import { format } from "date-fns/format";
import { parseISO } from "date-fns/parseISO";
import React from "react";

import { Transaction } from "models";
import { TransactionTagPanel } from "./TransactionTagPanel";
import { Amount } from "components/Amount";
import { useTransactionList } from "components/TransactionListProvider";

export const TransactionRow: React.FC<TransactionRowProps> = (props) => {

    const { showNet } = useTransactionList();

    return (
        <>
            <tr className="clickable transaction-row" onClick={() => props.onClick(props.transaction)} title={props.transaction.notes}>
                <td>{format(parseISO(props.transaction.transactionTime), "dd/MM/yy")}</td>
                <td className="description" colSpan={props.colspan}>{props.transaction.description}</td>
                <td className="d-none d-md-table-cell">{props.transaction.location}</td>
                <td className="d-none d-md-table-cell">{props.transaction.accountHolderName}</td>
                <td>
                    <Amount amount={props.transaction.amount} minus />
                    {showNet && props.transaction.amount !== props.transaction.netAmount &&
                        <span className="net-amount">(<Amount amount={props.transaction.netAmount} minus />)</span>
                    }
                </td>
                <TransactionTagPanel as="td" className="d-none d-md-table-cell" transaction={props.transaction} />
            </tr>
        </>
    );
}

export interface TransactionRowProps {
    transaction: Transaction;
    colspan?: number;
    onClick: (transaction: Transaction) => void;
}
