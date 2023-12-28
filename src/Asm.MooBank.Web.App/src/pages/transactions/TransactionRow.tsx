import React, { useState } from "react";
import { format } from "date-fns/format";
import { parseISO } from "date-fns/parseISO";

import { Transaction, TransactionOffset, TransactionSplit } from "models";
import { TransactionDetails } from "./TransactionDetails";
import { TransactionTagPanel } from "./TransactionTagPanel";
import { useUpdateTransaction } from "services";
import { formatCurrency } from "@andrewmclachlan/mooapp";

export const TransactionRow: React.FC<TransactionRowProps> = (props) => {

    const [showDetails, setShowDetails] = useState(false);

    const updateTransaction = useUpdateTransaction();

    const onSave = (notes: string, splits?: TransactionSplit[], offsetBy?: TransactionOffset[]) => {
        
        updateTransaction.mutate([{ accountId: props.transaction.accountId, transactionId: props.transaction.id }, { notes, splits}]);
        setShowDetails(false);
    }

    return (
        <>
            <TransactionDetails transaction={props.transaction} show={showDetails} onHide={() => setShowDetails(false)} onSave={onSave} />
            <tr className="clickable transaction-row" onClick={() => setShowDetails(true)} title={props.transaction.notes}>
                <td>{format(parseISO(props.transaction.purchaseDate ?? props.transaction.transactionTime), "dd/MM/yyyy")}</td>
                <td className="description" colSpan={props.colspan}>{props.transaction.description}</td>
                <td>{props.transaction.location}</td>
                <td>{props.transaction.accountHolderName}</td>
                <td className="amount">{formatCurrency(props.transaction.amount)}</td>
                <TransactionTagPanel as="td" transaction={props.transaction} />
            </tr>
        </>
    );
}

export interface TransactionRowProps {
    transaction: Transaction;
    colspan?: number
}