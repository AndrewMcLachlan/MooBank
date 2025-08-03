import { format } from "date-fns/format";
import { parseISO } from "date-fns/parseISO";
import React from "react";

import { Transaction } from "models";
import { TransactionTagPanel } from "./TransactionTagPanel";
import { Amount } from "components/Amount";
import { useTransactionList } from "components/TransactionListProvider";
import { isSameDay } from "date-fns";

export const TransactionRow: React.FC<TransactionRowProps> = ({transaction, onClick, previousDate}) => {

    const { showNet } = useTransactionList();

    return (
        <>
        { ((!previousDate || !isSameDay(previousDate, transaction.transactionTime)) &&
            <tr className="group-row transaction-date-row">
                <td colSpan={2} className="d-table-cell d-md-none">
                    {format(parseISO(transaction.transactionTime), "dd MMM yyyy")}
                </td>
                <td colSpan={6} className="d-none d-md-table-cell">
                    {format(parseISO(transaction.transactionTime), "dd MMM yyyy")}
                </td>
            </tr>
        )}
            <tr className="clickable transaction-row" onClick={() => onClick(transaction)} title={transaction.notes}>
                <td className="description d-none d-md-table-cell" colSpan={2}>{transaction.description}</td>
                <td className="description d-table-cell d-md-none" >{transaction.description}</td>
                <td className="d-none d-md-table-cell">{transaction.location}</td>
                <td className="d-none d-md-table-cell">{transaction.accountHolderName}</td>
                <td>
                    <Amount amount={transaction.amount} minus />
                    {showNet && transaction.amount !== transaction.netAmount &&
                        <span className="net-amount">(<Amount amount={transaction.netAmount} minus />)</span>
                    }
                </td>
                <TransactionTagPanel as="td" className="d-none d-md-table-cell" transaction={transaction} />
            </tr>
        </>
    );
}

export interface TransactionRowProps {
    transaction: Transaction;
    colspan?: number;
    onClick: (transaction: Transaction) => void;
    previousDate?: Date;
}
