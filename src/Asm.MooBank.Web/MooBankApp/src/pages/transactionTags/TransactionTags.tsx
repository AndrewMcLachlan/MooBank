import "./TransactionTags.scss";

import React, { useEffect, useState } from "react";
import { Col, Row, Table } from "react-bootstrap";

import { TransactionTagRow } from "./TransactionTagRow";

import { ClickableIcon } from "@andrewmclachlan/mooapp";
import { Pagination, TransactionTagPanel } from "components";
import { TransactionTag, sortTags } from "models";
import { useCreateTag, useTags } from "services";
import { Page } from "layouts";
import { getNumberOfPages } from "helpers/paging";
import { sortDirection } from "store/state";
import { TagsHeader } from "./TagsHeader";
import { changeSortDirection } from "helpers/sorting";
import { SearchBox } from "components/SearchBox";

export const TransactionTags: React.FC = () => {

    const { newTag, pagedTags, tagsList, addTag, createTag, removeTag, nameChange, pageNumber, numberOfPages, pageChange, totalTags, sortDirection, setSortDirection, keyUp, search, setSearch } = useComponentState();

    return (
        <Page title="Tags">
            <TagsHeader />
            <Page.Content>
                <Row>
                    <Col xl={6}>
                        <SearchBox value={search} onChange={(v) => setSearch(v)} />
                    </Col>
                </Row>
                <Table striped bordered={false} borderless className="transaction-tags">
                    <thead>
                        <tr>
                            <th className={`column-15 sortable ${sortDirection.toLowerCase()}`} onClick={() => setSortDirection(changeSortDirection(sortDirection))}>Name</th>
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
    const [filteredTags, setFilteredTags] = useState<TransactionTag[]>([]);
    const [pagedTags, setPagedTags] = useState<TransactionTag[]>([]);

    const [pageNumber, setPageNumber] = useState<number>(1);
    const [pageSize, setPageSize] = useState<number>(20);

    const [sortDirection, setSortDirection] = useState<sortDirection>("Ascending");
    const [search, setSearch] = useState("");

    const numberOfPages = getNumberOfPages(filteredTags.length, pageSize);
    const totalTags = filteredTags.length;
    const pageChange = (_current: number, newPage: number) => setPageNumber(newPage);

    useEffect(() => {
        setFilteredTags(fullTagsList.filter(t => t.name.toLocaleLowerCase().includes(search.toLocaleLowerCase())));
    }, [fullTagsList, search]);

    useEffect(() => {
        setPagedTags(filteredTags.sort(sortTags(sortDirection)).slice((pageNumber - 1) * pageSize, ((pageNumber - 1) * pageSize) + pageSize));
    }, [filteredTags, sortDirection, pageNumber]);

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
        sortDirection, setSortDirection,

        keyUp,
        search, setSearch
    };
}