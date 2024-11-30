import "./Tags.scss";

import React, { useEffect, useState } from "react";

import { TransactionTagRow } from "./TagRow";

import { changeSortDirection, getNumberOfPages, Pagination, SaveIcon, SearchBox, Section, SectionTable, SortDirection } from "@andrewmclachlan/mooapp";
import { TagPanel } from "components";
import { sortTags, Tag } from "models";
import { useCreateTag, useTags } from "services";
import { TagsPage } from "./TagsPage";

export const TransactionTags: React.FC = () => {

    const { newTag, pagedTags, tagsList, addTag, createTag, removeTag, nameChange, pageNumber, numberOfPages, pageChange, totalTags, sortDirection, setSortDirection, keyUp, search, setSearch, isLoading } = useComponentState();

    return (
        <TagsPage>
            <Section>
                <SearchBox value={search} onChange={(v: string) => setSearch(v)} />
            </Section>
            <SectionTable striped className="transaction-tags">
                <thead>
                    <tr>
                        <th className={`column-15 sortable ${sortDirection.toLowerCase()}`} onClick={() => setSortDirection(changeSortDirection(sortDirection))}>Name</th>
                        <th>Tags</th>
                        <th className="column-5"></th>
                    </tr>
                </thead>
                <tbody>
                    <tr>
                        <td><input type="text" placeholder="Tag name" value={newTag.name} onChange={nameChange} className="form-control" /></td>
                        <TagPanel as="td" selectedItems={newTag.tags} items={tagsList} onAdd={addTag} onCreate={createTag} onRemove={removeTag} allowCreate={false} alwaysShowEditPanel={true} onKeyUp={keyUp} />
                        <td className="row-action"><span onClick={createTag}><SaveIcon /></span></td>
                    </tr>
                    {pagedTags.map((t, i) => <TransactionTagRow key={`${i}${(t?.id ?? "")}`} tag={t} />)}
                </tbody>
                {!isLoading &&
                    <tfoot>
                        <tr>
                            <td colSpan={1} className="page-totals">Page {pageNumber} of {numberOfPages} ({totalTags} tags)</td>
                            <td colSpan={2}>
                                <Pagination pageNumber={pageNumber} numberOfPages={numberOfPages} onChange={pageChange} />
                            </td>
                        </tr>
                    </tfoot>
                }
            </SectionTable>
        </TagsPage>
    );
}

const useComponentState = () => {

    const blankTag = { id: 0, name: "", tags: [] } as Tag;

    const { data: allTags, isLoading } = useTags();

    const createTransactionTag = useCreateTag();

    const [pageNumber, setPageNumber] = useState<number>(1);
    const [pageSize, _setPageSize] = useState<number>(20);

    const [newTag, setNewTag] = useState(blankTag);
    const [tagsList, setTagsList] = useState<Tag[]>([]);
    const [filteredTags, setFilteredTags] = useState<Tag[]>([]);
    //@ts-ignore
    const [pagedTags, setPagedTags] = useState<Tag[] | undefined[]>(Array.from({ length: pageSize }).map(():any => undefined));

    const [sortDirection, setSortDirection] = useState<SortDirection>("Ascending");
    const [search, setSearch] = useState("");

    const numberOfPages = getNumberOfPages(filteredTags.length, pageSize);
    const totalTags = filteredTags.length;
    const pageChange = (_current: number, newPage: number) => setPageNumber(newPage);

    useEffect(() => {

        setPageNumber(1);

        const searchTerm = search.toLocaleLowerCase();
        if (searchTerm === "") {
            setFilteredTags(allTags ?? []);
            return;
        }

        const matchingTags = allTags?.filter(t => t?.name.toLocaleLowerCase().includes(searchTerm)) ?? [];
        const matchingSubTags = allTags?.filter(t => !matchingTags.some(t2 => t2.id === t.id) && t?.tags.some(t2 => matchingTags.some(t3 => t3.id === t2.id)));
        setFilteredTags(matchingTags.concat(matchingSubTags));
    }, [JSON.stringify(allTags), search]);

    useEffect(() => {
        if (isLoading) {
            //@ts-ignore
            setPagedTags(Array.from({ length: pageSize }).map(():any => undefined));
            return;
        }
        setPagedTags(filteredTags.sort(sortTags(sortDirection)).slice((pageNumber - 1) * pageSize, ((pageNumber - 1) * pageSize) + pageSize));
    }, [filteredTags, sortDirection, pageNumber]);

    useEffect(() => {
        if (!allTags) return;
        setTagsList(allTags.filter((t) => !newTag.tags.some((tt) => t.id === tt.id)));
    }, [newTag.tags, allTags]);

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

    const addTag = (tag: Tag) => {

        if (!tag.id) return;

        newTag.tags.push(tag);
        setNewTag(newTag);
    }

    const removeTag = (tag: Tag) => {

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
        tagsList,
        isLoading,

        createTag,
        createSubTag,
        addTag,
        removeTag,

        nameChange,

        pagedTags,
        pageNumber, numberOfPages, pageChange, totalTags, pageSize,
        sortDirection, setSortDirection,

        keyUp,
        search, setSearch
    };
}
