import React, { useEffect, useMemo, useState } from "react";

import { TagPanel } from "components";
import type { TagPanelProps } from "components";

import type { Tag } from "api/types.gen";
import { useAddSubTag } from "../-hooks/useAddSubTag";
import { useCreateTag } from "hooks/useCreateTag";
import { useRemoveSubTag } from "../-hooks/useRemoveSubTag";
import { useTags } from "hooks/useTags";
import { getParents, getRelatableTags } from "../-utils/tagRelationships";

export const TagParentPanel: React.FC<TagParentPanelProps> = ({ tag, ...rest }) => {

    const { data: allTags } = useTags();

    const { parents, addParent, removeParent, createParent } = useParentEvents(tag, allTags);

    const items = useMemo(() =>
        getRelatableTags(allTags ?? [], tag.id).filter((t) => !parents.some((p) => p.id === t.id)),
    [allTags, parents, tag.id]);

    return (
        <TagPanel {...rest} selectedItems={parents} items={items} onAdd={addParent} onRemove={removeParent} onCreate={createParent} allowCreate={true} />
    );
}

const useParentEvents = (tag: Tag, allTags: Tag[] | undefined) => {

    const storedParents = useMemo(() => getParents(allTags ?? [], tag.id), [allTags, tag.id]);

    const [parents, setParents] = useState(storedParents);

    useEffect(() => {
        setParents(storedParents);
    }, [storedParents]);

    const addSubTag = useAddSubTag();
    const removeSubTag = useRemoveSubTag();
    const createTag = useCreateTag();

    const addParent = (parent: Tag) => {
        if (!parent.id) return;

        setParents((current) => current.concat([parent]));
        addSubTag.mutate({ path: { id: parent.id, subTagId: tag.id } })
            .catch(() => setParents((current) => current.filter((p) => p.id !== parent.id)));
    }

    const removeParent = (parent: Tag) => {
        if (!parent.id) return;

        setParents((current) => current.filter((p) => p.id !== parent.id));
        removeSubTag.mutate({ path: { id: parent.id, subTagId: tag.id } })
            .catch(() => setParents((current) => current.concat([parent])));
    }

    const createParent = (name: string) => {
        createTag.mutate({ id: 0, name, tags: [tag] } as Tag);
    }

    return {
        parents,
        addParent,
        removeParent,
        createParent,
    };
}

export interface TagParentPanelProps extends Partial<Pick<TagPanelProps, "id" | "as" | "alwaysShowEditPanel">> {
    tag: Tag;
}
