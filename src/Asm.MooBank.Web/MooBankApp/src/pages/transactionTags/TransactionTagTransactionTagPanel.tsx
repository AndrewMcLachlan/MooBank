import React, { useState, useEffect } from "react";

import { TransactionTagPanel, TransactionTagPanelProps } from "components";

import { TransactionTag } from "models";
import { useAddSubTag, useCreateTag, useRemoveSubTag, useTags } from "services";

export const TransactionTagTransactionTagPanel: React.FC<TransactionTagTransactionTagPanelProps> = ({tag, ...rest}) => {

    const tagRow = useTagEvents(tag);

    const {data: fullTagsList} = useTags();

    const [tagsList, setTagsList] = useState<TransactionTag[]>([]);

    useEffect(() => {
        if (!fullTagsList) return;
        setTagsList(fullTagsList.filter((t) => t.id !== tag.id && tagRow.tags && !tagRow.tags.some((tt) => t.id === tt.id)));
    }, [tagRow.tags, fullTagsList]);

    return (
        <TransactionTagPanel {...rest} selectedItems={tagRow.tags} items={tagsList} onAdd={tagRow.addTag} onRemove={tagRow.removeTag} onCreate={tagRow.createTag} allowCreate={true} />
    );
}

export const useTagEvents = (tag: TransactionTag) => {

    const [tags, setTags] = useState(tag.tags);

    const addSubTag = useAddSubTag();
    const removeSubTag = useRemoveSubTag();
    const createTransactionTag = useCreateTag();

    useEffect(() => {
        setTags(tag.tags);
    }, [tag.tags]);

    const createTag = (name: string) => {
        createTransactionTag.mutate({ name }, {
            onSuccess: (data) => {
                addSubTag.mutate({ tagId: tag.id, subTagId: data.id });
            }
        });
    }

    const addTag = (subTag: TransactionTag) => {

        if (!subTag.id) return;

        addSubTag.mutate({ tagId: tag.id, subTagId: subTag.id });
        setTags(tags.concat([subTag]));
    }

    const removeTag = (subTag: TransactionTag) => {

        if (!subTag.id) return;   

        removeSubTag.mutate({ tagId: tag.id, subTagId: subTag.id });
        setTags(tags.filter((t) => t.id !== subTag.id));
    }

    return {
        createTag,
        addTag,
        removeTag,
        tags,
    };
}

export interface TransactionTagTransactionTagPanelProps extends Partial<Pick<TransactionTagPanelProps, "as" | "alwaysShowEditPanel">> {
    tag: TransactionTag;
}