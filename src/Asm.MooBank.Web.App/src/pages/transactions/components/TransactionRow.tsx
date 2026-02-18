import { format } from "date-fns/format";
import { parseISO } from "date-fns/parseISO";
import React from "react";

import type { Transaction } from "api/types.gen";
import { TransactionTagPanel } from "./TransactionTagPanel";
import { Amount } from "components/Amount";
import { useTransactionList } from "components/TransactionListProvider";
import { isSameDay } from "date-fns";
import { TransactionDateRow } from "./TransactionDateRow";

export const TransactionRow: React.FC<TransactionRowProps> = ({ transaction, onClick, previousDate, compact }) => {

    const { showNet } = useTransactionList();

    return (
        <>
            {(!previousDate || !isSameDay(previousDate, transaction.transactionTime)) &&
                <TransactionDateRow colspan={compact ? 2 : 6} date={format(parseISO(transaction.transactionTime), "dd MMM yyyy")} />
            }
            <tr className="clickable transaction-row" onClick={() => onClick(transaction)} title={transaction.notes}>
                <td className="description" colSpan={compact ? 1 : 2}>{transaction.description}</td>
                <td hidden={compact}>{transaction.location}</td>
                <td hidden={compact}>{transaction.accountHolderName}</td>
                <td className="transaction-amount">
                    <Amount amount={transaction.amount} minus />
                    {showNet && transaction.amount !== transaction.netAmount &&
                        <span className="net-amount">(<Amount amount={transaction.netAmount} minus />)</span>
                    }
                </td>
                <TransactionTagPanel hidden={compact} as="td" transaction={transaction} />
            </tr>
        </>
    );
}

export interface TransactionRowProps {
    transaction: Transaction;
    onClick: (transaction: Transaction) => void;
    previousDate?: Date;
    compact?: boolean;
}
