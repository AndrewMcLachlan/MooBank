import React, { useState, useEffect } from "react";
import format from "date-fns/format";
import parseISO from "date-fns/parseISO";

import { Transaction } from "models";
import { TransactionDetails } from "./TransactionDetails";
import { TransactionTransactionTagPanel } from "./TransactionTransactionTagPanel";
import { useUpdateTransaction } from "services";

export const TransactionRow: React.FC<TransactionRowProps> = (props) => {

    const [showDetails, setShowDetails] = useState(false);

    const updateTransaction = useUpdateTransaction();

    const onSave = (notes: string, offsetBy?: Transaction) => {
        
        updateTransaction.mutate([{ accountId: props.transaction.accountId, transactionId: props.transaction.id }, { notes, offsetByTransactionId: offsetBy.id }]);
        setShowDetails(false);
    }

    return (
        <>
            <TransactionDetails transaction={props.transaction} show={showDetails} onHide={() => setShowDetails(false)} onSave={onSave} />
            <tr className="clickable transaction-row" onClick={() => setShowDetails(true)} title={props.transaction.notes}>
                <td>{format(parseISO(props.transaction.transactionTime), "yyyy-MM-dd")}</td>
                <td className="description" colSpan={props.colspan}>{props.transaction.description}</td>
                <td className="amount">{props.transaction.amount.toLocaleString("en-AU", { minimumFractionDigits: 2, maximumFractionDigits: 2 })}</td>
                <TransactionTransactionTagPanel as="td" transaction={props.transaction} />
            </tr>
        </>
    );
}

export interface TransactionRowProps {
    transaction: Transaction;
    colspan?: number
}