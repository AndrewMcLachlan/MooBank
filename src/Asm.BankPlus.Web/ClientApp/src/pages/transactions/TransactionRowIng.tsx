import React, { useState, useEffect } from "react";
import moment from "moment";

import { TransactionTag } from "../../models";
import { TagPanel } from "../../components";
import { TransactionRowProps } from "./TransactionRow";
import { useAddTransactionTag, useCreateTag, useRemoveTransactionTag, useTags } from "../../services";

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
            <td>{moment(props.transaction.transactionTime).format("YYYY-MM-DD")}</td>
            <td>{props.transaction.description}</td>
            <td>{props.transaction.amount.toLocaleString("en-AU", { minimumFractionDigits: 2, maximumFractionDigits: 2 })}</td>
            <TagPanel as="td" selectedItems={transactionRow.tags} allItems={tagsList} textField="name" valueField="id" onAdd={transactionRow.addTag} onRemove={transactionRow.removeTag} onCreate={transactionRow.createTag} allowCreate={true} />
        </tr>
    );
}

TransactionRowIng.displayName = "TransactionRowIng";

function useTransactionRowEvents(props: TransactionRowProps) {

    const [tags, setTags] = useState(props.transaction.tags);

    const addTransactionTag = useAddTransactionTag();
    const removeTransactionTag = useRemoveTransactionTag();
    const createTransactionTag = useCreateTag();

    useEffect(() => {
        setTags(props.transaction.tags);
    }, [props.transaction.tags]);

    const createTag = (name: string) => {
        createTransactionTag.mutate({ name }, {
            onSuccess: (data) => {
                addTransactionTag.mutate({ transactionId: props.transaction.id, tag: data});
            }
        });
    }

    const addTag = (tag: TransactionTag) => {

        if (!tag.id) return;

        addTransactionTag.mutate({ transactionId: props.transaction.id, tag });
        setTags(tags.concat([tag]));
    }

    const removeTag = (tag: TransactionTag) => {

        if (!tag.id) return;

        removeTransactionTag.mutate({ transactionId: props.transaction.id, tag });
        setTags(tags.filter((t) => t.id !== tag.id));
    }

    return {
        createTag,
        addTag,
        removeTag,
        tags,
    };
}