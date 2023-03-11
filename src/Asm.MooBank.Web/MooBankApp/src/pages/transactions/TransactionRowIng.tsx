import React, { useState, useEffect } from "react";
import format from "date-fns/format";
import parseISO from "date-fns/parseISO";

import { TagPanel } from "@andrewmclachlan/mooapp";
import { TransactionRowProps } from "./TransactionRow";
import { useTags } from "../../services";
import { useTransactionRowEvents } from "./TransactionRow";

export const TransactionRowIng: React.FC<TransactionRowProps> = (props) => {

    const transactionRow = useTransactionRowEvents(props);

    const fullTagsListQuery = useTags();

    const [tagsList, setTagsList] = useState([]);

    useEffect(() => {
        if (!fullTagsListQuery.data) return;
        setTagsList(fullTagsListQuery.data.filter((t) => !transactionRow.tags.some((tt) => t.id === tt.id)));
    }, [transactionRow.tags, fullTagsListQuery.data]);

    return (
        <tr>
            <td>{format(parseISO(props.transaction.transactionTime), "yyyy-MM-dd")}</td>
            <td>{props.transaction.description}</td>
            <td>{props.transaction.amount.toLocaleString("en-AU", { minimumFractionDigits: 2, maximumFractionDigits: 2 })}</td>
            <TagPanel as="td" selectedItems={transactionRow.tags} allItems={tagsList} textField="name" valueField="id" onAdd={transactionRow.addTag} onRemove={transactionRow.removeTag} onCreate={transactionRow.createTag} allowCreate={true} />
        </tr>
    );
}

TransactionRowIng.displayName = "TransactionRowIng";
