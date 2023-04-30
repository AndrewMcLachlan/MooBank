﻿import React, { useState, useEffect } from "react";

import { TransactionTagPanel } from "components";

import { Transaction, TransactionTag } from "models";
import { useAccount } from "components";
import { useAddTransactionTag, useCreateTag, useRemoveTransactionTag, useTags } from "services";

export const TransactionTransactionTagPanel: React.FC<TransactionTransactionTagPanelProps> = (props) => {

    const transactionRow = useTransactionRowEvents(props);

    const fullTagsListQuery = useTags();

    const [tagsList, setTagsList] = useState<TransactionTag[]>([]);

    useEffect(() => {
        if (!fullTagsListQuery.data) return;
        setTagsList(fullTagsListQuery.data.filter((t) => !transactionRow.tags.some((tt) => t.id === tt.id)));
    }, [transactionRow.tags, fullTagsListQuery.data]);

    return (
        <TransactionTagPanel as={props.as} selectedItems={transactionRow.tags} items={tagsList} onAdd={transactionRow.addTag} onRemove={transactionRow.removeTag} onCreate={transactionRow.createTag} allowCreate={true} />
    );
}

export const useTransactionRowEvents = (props: TransactionTransactionTagPanelProps) => {

    const account = useAccount();

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
                addTransactionTag.mutate({ accountId: account.id, transactionId: props.transaction.id, tag: data });
            }
        });
    }

    const addTag = (tag: TransactionTag) => {

        if (!tag.id) return;

        addTransactionTag.mutate({ accountId: account.id, transactionId: props.transaction.id, tag });
        setTags(tags.concat([tag]));
    }

    const removeTag = (tag: TransactionTag) => {

        if (!tag.id) return;

        removeTransactionTag.mutate({ accountId: account.id, transactionId: props.transaction.id, tag });
        setTags(tags.filter((t) => t.id !== tag.id));
    }

    return {
        createTag,
        addTag,
        removeTag,
        tags,
    };
}

export interface TransactionTransactionTagPanelProps {
    as?: string;
    transaction: Transaction;
}