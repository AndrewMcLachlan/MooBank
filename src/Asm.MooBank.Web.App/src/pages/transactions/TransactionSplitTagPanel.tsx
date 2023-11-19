import React, { useState, useEffect } from "react";

import { TagPanel } from "components";

import { Tag, TransactionSplit } from "models";
import { useCreateTag, useTags } from "services";
import { emptyGuid } from "@andrewmclachlan/mooapp";


export const TransactionSplitTagPanel: React.FC<TransactionSplitPanelProps> = (props) => {

    const [transactionSplit, setTransactionSplit] = useState<TransactionSplit>(props.transactionSplit);

    useEffect(() => {
        setTransactionSplit(props.transactionSplit);
    }, [props.transactionSplit]);

    const createTransactionTag = useCreateTag();

    const createTag = (name: string) => {
        createTransactionTag.mutate({ name }, {
            onSuccess: (data) => {
                const newSplit = { ...transactionSplit, tags: [...transactionSplit.tags, data]};
                setTransactionSplit(newSplit);
                props.onChange(newSplit);
            }
        });
    }

    const addTag = (tag: Tag) => {

        if (!tag.id) return;

        const newSplit = { ...transactionSplit, tags: [...transactionSplit.tags, tag]};
        setTransactionSplit(newSplit);
        props.onChange(newSplit);
    }

    const removeTag = (tag: Tag) => {

        if (!tag.id) return;

        const newSplit = { ...transactionSplit, tags: transactionSplit.tags.filter((t) => t.id !== tag.id)}
        setTransactionSplit(newSplit);
        props.onChange(newSplit);
    }

    const fullTagsListQuery = useTags();

    const [tagsList, setTagsList] = useState<Tag[]>([]);

    useEffect(() => {
        if (!fullTagsListQuery.data) return;
        setTagsList(fullTagsListQuery.data.filter((t) => !transactionSplit.tags.some((tt) => t.id === tt.id)));
    }, [transactionSplit.tags, fullTagsListQuery.data]);

    return (
        <TagPanel as={props.as} selectedItems={transactionSplit.tags} items={tagsList} onAdd={addTag} onRemove={removeTag} onCreate={createTag} allowCreate={true} alwaysShowEditPanel={props.alwaysShowEditPanel}  />
    );
}

TransactionSplitTagPanel.defaultProps = {
    alwaysShowEditPanel: false,
}

export interface TransactionSplitPanelProps {
    as?: string;
    alwaysShowEditPanel?: boolean;
    transactionId: string;
    transactionSplit: TransactionSplit;
    onChange: (transactionSplit: TransactionSplit) => void;
}