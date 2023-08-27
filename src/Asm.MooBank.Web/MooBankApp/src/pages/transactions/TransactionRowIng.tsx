﻿import React, { useState } from "react";
import format from "date-fns/format";
import parseISO from "date-fns/parseISO";

import { TransactionRow, TransactionRowProps } from "./TransactionRow";
import { TransactionTransactionTagPanel } from "./TransactionTransactionTagPanel";
import { TransactionDetails } from "./TransactionDetails";
import { useUpdateTransaction } from "services";
import { Transaction } from "models";
import { formatCurrency } from "@andrewmclachlan/mooapp";

export const TransactionRowIng: React.FC<TransactionRowProps> = (props) => {

    const [showDetails, setShowDetails] = useState(false);

    const updateTransaction = useUpdateTransaction();

    const onSave = (notes: string, offsetBy?: Transaction) => {
        
        updateTransaction.mutate([{ accountId: props.transaction.accountId, transactionId: props.transaction.id }, { notes, offsetByTransactionId: offsetBy?.id }]);
        setShowDetails(false);
    }

    if (!props.transaction.extraInfo) {
        return <TransactionRow {...props} colspan={3} />;
    }

    return (
        <>
            <TransactionDetails transaction={props.transaction} show={showDetails} onHide={() => setShowDetails(false)} onSave={onSave} />
            <tr className="clickable transaction-row" onClick={() => setShowDetails(true)} title={props.transaction.notes}>
                <td>{props.transaction.extraInfo.purchaseDate ? format(parseISO(props.transaction.extraInfo.purchaseDate), "dd/MM/yyyy") : format(parseISO(props.transaction.transactionTime), "dd/MM/yyyy")}</td>
                <td>{props.transaction.extraInfo.description}</td>
                <td>{props.transaction.extraInfo.location}</td>
                <td>{props.transaction.extraInfo.who}</td>
                <td className="amount">{formatCurrency(props.transaction.amount)}</td>
                <TransactionTransactionTagPanel as="td" transaction={props.transaction} />
            </tr>
        </>
    );
}

TransactionRowIng.displayName = "TransactionRowIng";
