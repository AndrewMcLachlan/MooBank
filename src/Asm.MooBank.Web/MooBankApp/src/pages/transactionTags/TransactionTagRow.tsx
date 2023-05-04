import React, { useState, useEffect } from "react";

import { TransactionTag } from "models";
import { ClickableIcon, EditColumn } from "@andrewmclachlan/mooapp";
import { TransactionTagPanel } from "components";
import { useAddSubTag, useCreateTag, useDeleteTag, useRemoveSubTag, useTags, useUpdateTag } from "services";

export const TransactionTagRow: React.FC<TransactionTagRowProps> = (props) => {

    const { tag, ...transactionRow } = useTransactionTagRowEvents(props);

    const fullTagsListQuery = useTags();
    const fullTagsList = fullTagsListQuery.data ?? [];

    const [tagsList, setTagsList] = useState([]);

    useEffect(() => {
        setTagsList(fullTagsList.filter((t) => t.id !== props.tag.id && tag?.tags && !tag.tags.some((tt) => t.id === tt.id)));
    }, [tag, fullTagsList]);

    return (
        <tr>
            <EditColumn value={tag.name} onChange={(transactionRow.updateTag)} />
            <TransactionTagPanel as="td" selectedItems={tag.tags} items={tagsList} onAdd={transactionRow.addTag} onRemove={transactionRow.removeTag} onCreate={transactionRow.createTag} allowCreate={true} />
            <td className="row-action"><span onClick={transactionRow.deleteTag}><ClickableIcon icon="trash-alt" title="Delete" /></span></td>
        </tr>
    );
}

function useTransactionTagRowEvents(props: TransactionTagRowProps) {

    const [tag, setTag] = useState(props.tag);

    const createTransactionTag = useCreateTag();
    const updateTransactionTag = useUpdateTag();
    const deleteTransactionTag = useDeleteTag();
    const addSubTag = useAddSubTag();
    const removeSubTag = useRemoveSubTag();

    useEffect(() => {
        setTag(props.tag);
    }, [props.tag]);

    const createTag = (name: string) => {
        createTransactionTag.mutate({ name }, {
            onSuccess: (data) => {
                addSubTag.mutate({ tagId: props.tag.id, subTagId: data.id });
            }
        });
    }

    const deleteTag = () => {

        if (window.confirm("Deleting this tag will remove it from all rules and transactions. Are you sure?")) {
            deleteTransactionTag.mutate({ id: props.tag.id });
        }
    }

    const addTag = (subTag: TransactionTag) => {

        if (!subTag.id) return;

        addSubTag.mutate({ tagId: tag.id, subTagId: subTag.id });
        setTag({ ...subTag, tags: tag.tags.concat([subTag]) });
    }

    const removeTag = (subTag: TransactionTag) => {

        if (!subTag.id) return;   

        removeSubTag.mutate({ tagId: props.tag.id, subTagId: tag.id });
        setTag({ ...tag, tags: tag.tags.filter((t) => t.id !== subTag.id) });
    }

    const updateTag = (name: string) => {
        updateTransactionTag.mutate({ ...tag, name });
        setTag({ ...tag, name });
    }

    return {
        createTag,
        deleteTag,
        addTag,
        removeTag,
        updateTag,
        tag,
    };
}

export interface TransactionTagRowProps {
    tag: TransactionTag;
}