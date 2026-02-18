import React, { useState, useEffect } from "react";

import { TagPanel, TagPanelProps } from "components";

import type { Tag } from "api/types.gen";
import { useAddSubTag, useCreateTag, useRemoveSubTag, useTags } from "../../services";

export const TransactionTagTransactionTagPanel: React.FC<TransactionTagTransactionTagPanelProps> = ({tag, ...rest}) => {

    const tagRow = useTagEvents(tag);

    const {data: fullTagsList} = useTags();

    const [tagsList, setTagsList] = useState<Tag[]>([]);

    useEffect(() => {
        if (!fullTagsList) return;
        setTagsList(fullTagsList.filter((t) => t.id !== tag.id && tagRow.tags && !tagRow.tags.some((tt) => t.id === tt.id)));
    }, [tagRow.tags, fullTagsList]);

    return (
        <TagPanel {...rest} selectedItems={tagRow.tags} items={tagsList} onAdd={tagRow.addTag} onRemove={tagRow.removeTag} onCreate={tagRow.createTag} allowCreate={true} />
    );
}

export const useTagEvents = (tag: Tag) => {

    const [tags, setTags] = useState(tag.tags);

    const addSubTag = useAddSubTag();
    const removeSubTag = useRemoveSubTag();
    const createTransactionTag = useCreateTag();

    useEffect(() => {
        setTags(tag.tags);
    }, [tag.tags]);

    const createTag = async (name: string) => {
        const data = await createTransactionTag.mutateAsync({ name });
        addSubTag.mutate({ path: { id: tag.id, subTagId: data.id } });
    }

    const addTag = (subTag: Tag) => {

        if (!subTag.id) return;

        addSubTag.mutate({ path: { id: tag.id, subTagId: subTag.id } });
        setTags(tags.concat([subTag]));
    }

    const removeTag = (subTag: Tag) => {

        if (!subTag.id) return;

        removeSubTag.mutate({ path: { id: tag.id, subTagId: subTag.id } });
        setTags(tags.filter((t) => t.id !== subTag.id));
    }

    return {
        createTag,
        addTag,
        removeTag,
        tags,
    };
}

export interface TransactionTagTransactionTagPanelProps extends Partial<Pick<TagPanelProps, "id" | "as" | "alwaysShowEditPanel">> {
    tag: Tag;
}