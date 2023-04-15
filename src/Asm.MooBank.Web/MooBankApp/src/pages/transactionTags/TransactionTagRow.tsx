import React, { useState, useEffect } from "react";

import { TransactionTag } from "models";
import { ClickableIcon, TagPanel } from "@andrewmclachlan/mooapp";
import { useAddSubTag, useCreateTag, useDeleteTag, useRemoveSubTag, useTags } from "services";

export const TransactionTagRow: React.FC<TransactionTagRowProps> = (props) => {

    const transactionRow = useTransactionTagRowEvents(props);

    const fullTagsListQuery = useTags();
    const fullTagsList = fullTagsListQuery.data ?? [];

    const [tagsList, setTagsList] = useState([]);

    useEffect(() => {
        setTagsList(fullTagsList.filter((t) => t.id !== props.tag.id && transactionRow.tags && !transactionRow.tags.some((tt) => t.id === tt.id)));
    }, [transactionRow.tags, fullTagsList]);

    return (
        <tr>
            <td>{props.tag.name}</td>
            <TagPanel as="td" selectedItems={transactionRow.tags} allItems={tagsList} textField="name" valueField="id" onAdd={transactionRow.addTag} onRemove={transactionRow.removeTag} onCreate={transactionRow.createTag} allowCreate={true} />
            <td className="row-action"><span onClick={transactionRow.deleteTag}><ClickableIcon icon="trash-alt" title="Delete" /></span></td>
        </tr>
    );
}

function useTransactionTagRowEvents(props: TransactionTagRowProps) {

    const [tags, setTags] = useState(props.tag.tags);

    const createTransactionTag = useCreateTag();
    const deleteTransactionTag = useDeleteTag();
    const addSubTag = useAddSubTag();
    const removeSubTag = useRemoveSubTag();

    useEffect(() => {
        setTags(props.tag.tags);
    }, [props.tag.tags]);

    const createTag = (name: string) => {
        createTransactionTag.mutate({ name }, {
            onSuccess: (data) => {
                addSubTag.mutate({ tagId: props.tag.id, subTagId: data.id});
            }
        });
    }

    const deleteTag = () => {

        if (window.confirm("Deleting this tag will remove it from all rules and transactions. Are you sure?")) {
            deleteTransactionTag.mutate({ id: props.tag.id});
        }
    }

    const addTag = (tag: TransactionTag) => {

        if (!tag.id) return;

        addSubTag.mutate({ tagId: props.tag.id, subTagId: tag.id });
        setTags(tags.concat([tag]));
    }

    const removeTag = (tag: TransactionTag) => {

        if (!tag.id) return;

        removeSubTag.mutate({ tagId: props.tag.id, subTagId: tag.id});
        setTags(tags.filter((t) => t.id !== tag.id));
    }

    return {
        createTag,
        deleteTag,
        addTag,
        removeTag,
        tags,
    };
}

export interface TransactionTagRowProps {
    tag: TransactionTag;
}