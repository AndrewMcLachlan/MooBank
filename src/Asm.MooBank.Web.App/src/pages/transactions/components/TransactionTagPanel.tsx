import React, { useState, useEffect, useMemo } from "react";

import { TagPanel } from "components";

import { Transaction, Tag } from "models";
import { useAccount } from "components";
import { useAddTransactionTag, useCreateTag, useRemoveTransactionTag, useTags } from "services";

export const TransactionTagPanel: React.FC<TransactionTagPanelProps> = ({alwaysShowEditPanel = false, ...props}) => {

    const transactionRow = useTransactionRowEvents(props);

    const fullTagsListQuery = useTags();

    const [tagsList, setTagsList] = useState<Tag[]>([]);

    useEffect(() => {
        if (!fullTagsListQuery.data) return;
        setTagsList(fullTagsListQuery.data.filter((t) => !transactionRow.tags.some((tt) => t.id === tt.id)));
    }, [transactionRow.tags, fullTagsListQuery.data]);

    const selectedTags = useMemo(() => {
        if (!fullTagsListQuery.data) return [];
        return fullTagsListQuery.data.filter((t) => transactionRow.tags.some((tt) => t.id === tt.id));
    }, [transactionRow.tags, fullTagsListQuery.data]);

    if (props.hidden) return null;

    return (
        <TagPanel as={props.as} className={props.className} selectedItems={selectedTags} items={tagsList} onAdd={transactionRow.addTag} onRemove={transactionRow.removeTag} onCreate={transactionRow.createTag} allowCreate={true} alwaysShowEditPanel={alwaysShowEditPanel}  />
    );
}

export const useTransactionRowEvents = (props: TransactionTagPanelProps) => {

    const account = useAccount();

    const [tags, setTags] = useState(props.transaction.tags);

    const addTransactionTag = useAddTransactionTag();
    const removeTransactionTag = useRemoveTransactionTag();
    const createTransactionTag = useCreateTag();

    useEffect(() => {
        setTags(props.transaction.tags);
    }, [props.transaction.tags]);

    const createTag = async (name: string) => {
        const data = await createTransactionTag.mutateAsync({ name });
        addTransactionTag.mutate({ accountId: account.id, transactionId: props.transaction.id, tag: data });
    }

    const addTag = (tag: Tag) => {

        if (!tag.id) return;

        addTransactionTag.mutate({ accountId: account.id, transactionId: props.transaction.id, tag });
        setTags(tags.concat([tag]));
    }

    const removeTag = (tag: Tag) => {

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

export interface TransactionTagPanelProps {
    as?: string;
    alwaysShowEditPanel?: boolean;
    transaction: Transaction;
    className?: string;
    hidden?: boolean;
}
