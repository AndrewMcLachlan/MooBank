import "./TransactionTags.scss";

import React, { useEffect, useState } from "react";
import { Table } from "react-bootstrap";

import { TransactionTagRow } from "./TransactionTagRow";

import { ClickableIcon , TagPanel } from "@andrewmclachlan/mooapp";
import { TransactionTag } from "../../models";
import { useCreateTag, useTags } from "../../services";
import { Page } from "../../layouts";

export const TransactionTags: React.FC = () => {

    const { newTag, fullTagsList, tagsList, addTag, createTag, removeTag, nameChange } = useComponentState();

    return (
        <Page title="Tags">
            <Page.Header title="Transaction Tags" breadcrumbs={[["Transaction Tags", "/settings"]]} />
            <Page.Content>
                <Table striped bordered={false} borderless className="transaction-tags">
                    <thead>
                        <tr>
                            <th>Name</th>
                            <th>Tags</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr>
                            <td><input type="text" placeholder="Tag name" value={newTag.name} onChange={nameChange} className="form-control" /></td>
                            <td><TagPanel selectedItems={newTag.tags} allItems={tagsList} textField="name" valueField="id" onAdd={addTag} onCreate={createTag} onRemove={removeTag} allowCreate={false} alwaysShowEditPanel={true} /></td>
                            <td className="row-action"><span onClick={createTag}><ClickableIcon icon="check-circle" title="Save" size="xl" /></span></td>
                        </tr>
                        {fullTagsList && fullTagsList.map((t) => <TransactionTagRow key={t.id} tag={t} />)}
                    </tbody>
                </Table>
            </Page.Content>
        </Page>
    );
}

const useComponentState = () => {

    const blankTag = { id: 0, name: "", tags: [] } as TransactionTag;

    const fullTagsListQuery = useTags();
    const fullTagsList = fullTagsListQuery.data;

    const createTransactionTag = useCreateTag();

    const [newTag, setNewTag] = useState(blankTag);
    const [tagsList, setTagsList] = useState<TransactionTag[]>([]);

    useEffect(() => {
        if (!fullTagsListQuery.data) return;
        setTagsList(fullTagsListQuery.data.filter((t) => !newTag.tags.some((tt) => t.id === tt.id)));
    }, [newTag.tags, fullTagsListQuery.data]);

    const createTag = () => {
        createTransactionTag.mutate(newTag);
        setNewTag(blankTag);
    }

    const createSubTag = (name: string) => {
        createTransactionTag.mutate({ name });
    }

    const nameChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        setNewTag({ ...newTag, name: e.currentTarget.value });
    }

    const addTag = (tag: TransactionTag) => {

        if (!tag.id) return;

        newTag.tags.push(tag);
        setNewTag(newTag);
    }

    const removeTag = (tag: TransactionTag) => {

        if (!tag.id) return;

        newTag.tags = newTag.tags.filter((t) => t.id !== tag.id);
        setNewTag(newTag);
    }

    return {
        newTag,
        fullTagsList,
        tagsList,

        createTag,
        createSubTag,
        addTag,
        removeTag,

        nameChange,
    };
}