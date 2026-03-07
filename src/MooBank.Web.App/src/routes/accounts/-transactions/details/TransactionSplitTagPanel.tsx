import React, { useState, useMemo } from "react";

import { TagPanel } from "components";

import type { Tag, TransactionSplit } from "api/types.gen";
import { useCreateTag } from "hooks/useCreateTag";
import { useTags } from "hooks/useTags";

export const TransactionSplitTagPanel: React.FC<TransactionSplitPanelProps> = ({ alwaysShowEditPanel = false, ...props }) => {

    const [transactionSplit, setTransactionSplit] = useState<TransactionSplit>(props.transactionSplit);

    const createTransactionTag = useCreateTag();

    const createTag = async (name: string) => {
        const data = await createTransactionTag.mutateAsync({ name });
        const newSplit = { ...transactionSplit, tags: [...transactionSplit.tags, data]};
        setTransactionSplit(newSplit);
        props.onChange(newSplit);
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

    const tagsList = useMemo(() => {
        if (!fullTagsListQuery.data) return [];
        return fullTagsListQuery.data.filter((t) => !transactionSplit.tags.some((tt) => t.id === tt.id));
    }, [transactionSplit.tags, fullTagsListQuery.data]);

    const selectedTags = useMemo(() => {
        if (!fullTagsListQuery.data) return [];
        return fullTagsListQuery.data.filter((t) => transactionSplit.tags.some((tt) => t.id === tt.id));
    }, [transactionSplit.tags, fullTagsListQuery.data]);

    return (
        <TagPanel as={props.as} selectedItems={selectedTags} items={tagsList} onAdd={addTag} onRemove={removeTag} onCreate={createTag} allowCreate={true} alwaysShowEditPanel={alwaysShowEditPanel}  />
    );
}

export interface TransactionSplitPanelProps {
    as?: string;
    alwaysShowEditPanel?: boolean;
    transactionId: string;
    transactionSplit: TransactionSplit;
    onChange: (transactionSplit: TransactionSplit) => void;
}
