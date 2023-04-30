import React, { useState, useEffect } from "react";
import format from "date-fns/format";
import parseISO from "date-fns/parseISO";

import { TransactionRowProps } from "./TransactionRow";
import { TransactionTransactionTagPanel } from "./TransactionTransactionTagPanel";

export const TransactionRowIng: React.FC<TransactionRowProps> = (props) => {

    return (
        <tr>
            <td>{format(parseISO(props.transaction.transactionTime), "yyyy-MM-dd")}</td>
            <td>{props.transaction.description}</td>
            <td>{props.transaction.amount.toLocaleString("en-AU", { minimumFractionDigits: 2, maximumFractionDigits: 2 })}</td>
            <TransactionTransactionTagPanel as="td" transaction={props.transaction} />
        </tr>
    );
}

TransactionRowIng.displayName = "TransactionRowIng";
