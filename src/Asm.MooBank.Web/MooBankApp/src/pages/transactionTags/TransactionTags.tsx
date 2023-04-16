import "./TransactionTags.scss";

import React, { useEffect, useState } from "react";
import { Table } from "react-bootstrap";

import { TransactionTagRow } from "./TransactionTagRow";

import { ClickableIcon } from "@andrewmclachlan/mooapp";
import { Pagination, TransactionTagPanel } from "components";
import { TransactionTag, sort } from "models";
import { useCreateTag, useTags } from "services";
import { Page } from "layouts";
import { getNumberOfPages } from "helpers/paging";
import { sortDirection } from "store/state";

export const TransactionTags: React.FC = () => {

    const { newTag, pagedTags, tagsList, addTag, createTag, removeTag, nameChange, pageNumber, numberOfPages, pageChange, totalTags, sortDirection, changeSortDirection, keyUp } = useComponentState();

    return (
        <Page title="Tags">
            <Page.Header title="Transaction Tags" breadcrumbs={[["Transaction Tags", "/settings"]]} />
            <Page.Content>
                <Table striped bordered={false} borderless className="transaction-tags">
                    <thead>
                        <tr>
                            <th className={`column-15 sortable ${sortDirection.toLowerCase()}`} onClick={changeSortDirection}>Name</th>
                            <th>Tags</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr>
                            <td><input type="text" placeholder="Tag name" value={newTag.name} onChange={nameChange} className="form-control" /></td>
                            <TransactionTagPanel as="td" selectedItems={newTag.tags} items={tagsList} onAdd={addTag} onCreate={createTag} onRemove={removeTag} allowCreate={false} alwaysShowEditPanel={true} onKeyUp={keyUp} />
                            <td className="row-action"><span onClick={createTag}><ClickableIcon icon="check-circle" title="Save" size="xl" /></span></td>
                        </tr>
                        {pagedTags.map((t) => <TransactionTagRow key={t.id} tag={t} />)}
                    </tbody>
                    <tfoot>
                        <tr>
                            <td colSpan={1} className="page-totals">Page {pageNumber} of {numberOfPages} ({totalTags} tags)</td>
                            <td colSpan={2}>
                                <Pagination pageNumber={pageNumber} numberOfPages={numberOfPages} onChange={pageChange} />
                            </td>
                        </tr>
                    </tfoot>
                </Table>
            </Page.Content>
        </Page>
    );
}

const useComponentState = () => {

    const blankTag = { id: 0, name: "", tags: [] } as TransactionTag;

    const fullTagsListQuery = useTags();
    const fullTagsList = fullTagsListQuery.data ?? [];

    const createTransactionTag = useCreateTag();

    const [newTag, setNewTag] = useState(blankTag);
    const [tagsList, setTagsList] = useState<TransactionTag[]>([]);
    const [pagedTags, setPagedTags] = useState<TransactionTag[]>([]);

    const [pageNumber, setPageNumber] = useState<number>(1);
    const [pageSize, setPageSize] = useState<number>(20);

    const [sortDirection, setSortDirection] = useState<sortDirection>("Ascending");

    const numberOfPages = getNumberOfPages(fullTagsList.length, pageSize);
    const totalTags = fullTagsList.length;
    const pageChange = (_current: number, newPage: number) => setPageNumber(newPage);

    useEffect(() => {
        setPagedTags(fullTagsList.sort(sort(sortDirection)).slice((pageNumber - 1) * pageSize, ((pageNumber - 1) * pageSize) + pageSize));
    }, [fullTagsList, sortDirection, pageNumber]);

    const changeSortDirection = () => {
        setSortDirection(sortDirection === "Ascending" ? "Descending" : "Ascending");
    }

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

    const keyUp: React.KeyboardEventHandler<HTMLTableCellElement> = (e) => { 
        if (e.key === "Enter") {
            createTag();
        }
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

        pagedTags,
        pageNumber, numberOfPages, pageChange, totalTags,
        changeSortDirection,sortDirection,

        keyUp,
    };
}