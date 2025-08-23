import React, { useEffect, useState } from "react";

import { TransactionTagRow } from "./TagRow";

import { changeSortDirection, getNumberOfPages, PageSize, Pagination, PaginationControls, PaginationTh, SaveIcon, SearchBox, Section, SectionTable, SortableTh, SortDirection, useLocalStorage } from "@andrewmclachlan/moo-ds";
import { TagPanel } from "components";
import { sortTags, Tag } from "models";
import { useCreateTag, useTags } from "services";
import { TagsPage } from "./TagsPage";

export const TransactionTags: React.FC = () => {

    const blankTag = { id: 0, name: "", tags: [] } as Tag;

    const { data: allTags, isLoading } = useTags();

    const createTransactionTag = useCreateTag();

    const [pageNumber, setPageNumber] = useState<number>(1);
    const [pageSize, setPageSize] = useLocalStorage<number>("tags-page-size", 20);

    const [newTag, setNewTag] = useState(blankTag);
    const [tagsList, setTagsList] = useState<Tag[]>([]);
    const [filteredTags, setFilteredTags] = useState<Tag[]>([]);
    const [pagedTags, setPagedTags] = useState<Tag[] | undefined[]>(Array.from({ length: pageSize }).map((): any => undefined));

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
            setPagedTags(Array.from({ length: pageSize }).map((): any => undefined));
            return;
        }
        setPagedTags(filteredTags.sort(sortTags(sortDirection)).slice((pageNumber - 1) * pageSize, ((pageNumber - 1) * pageSize) + pageSize));
    }, [filteredTags, sortDirection, pageNumber, pageSize]);

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

    return (
        <TagsPage>
            <Section>
                <SearchBox value={search} onChange={(v: string) => setSearch(v)} />
            </Section>
            <SectionTable striped className="transaction-tags">
                <thead>
                    <tr>
                        <SortableTh className={`column-15 sortable ${sortDirection.toLowerCase()}`} sortField="name" sortDirection={sortDirection} onSort={() => setSortDirection(changeSortDirection(sortDirection))} field="name">Name</SortableTh>
                        <th>Tags</th>
                        <PaginationTh pageNumber={pageNumber} numberOfPages={numberOfPages} onChange={pageChange} />
                    </tr>
                </thead>
                <tbody>
                    <tr>
                        <td><input type="text" placeholder="Tag name" value={newTag.name} onChange={nameChange} className="form-control" /></td>
                        <TagPanel as="td" selectedItems={newTag.tags} items={tagsList} onAdd={addTag} onCreate={createTag} onRemove={removeTag} allowCreate={false} alwaysShowEditPanel={true} onKeyUp={keyUp} />
                        <td className="row-action column-5"><span onClick={createTag}><SaveIcon /></span></td>
                    </tr>
                    {pagedTags.map((t, i) => <TransactionTagRow key={`${i}${(t?.id ?? "")}`} tag={t} />)}
                </tbody>
                {!isLoading &&
                    <tfoot>
                        <tr>
                            <td colSpan={1} className="page-totals">Page {pageNumber} of {numberOfPages} ({totalTags} tags)</td>
                            <td colSpan={2}>
                                <PaginationControls>
                                    <PageSize value={pageSize} onChange={setPageSize} />
                                    <Pagination pageNumber={pageNumber} numberOfPages={numberOfPages} onChange={pageChange} />
                                </PaginationControls>
                            </td>
                        </tr>
                    </tfoot>
                }
            </SectionTable>
        </TagsPage>
    );
}
