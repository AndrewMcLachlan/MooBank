import { createFileRoute } from "@tanstack/react-router";
import React, { useMemo } from "react";
import { LoadingTableRows, Table } from "@andrewmclachlan/moo-ds";

import { PageSize, Pagination, PaginationControls, PaginationTh, SearchBox, Section, SortableTh, changeSortDirection, getNumberOfPages } from "@andrewmclachlan/moo-ds";
import { AccountPage, useAccount } from "components";

import { sortRules } from "utils/rules";
import { validateGridSearch, type GridSearchDefaults } from "utils/gridSearch";

import { useGridPageSize, useGridSearch, usePageInRange, useSearchTerm } from "hooks/useGridSearch";
import { useTags } from "hooks/useTags";
import { useRules } from "routes/accounts/-hooks/useRules";
import { useRunRules } from "routes/accounts/-hooks/useRunRules";
import { NewRule } from "./-rules/NewRule";
import { RuleRow } from "./-rules/RuleRow";

const rulesGridDefaults: GridSearchDefaults = { sortField: "description", sortDirection: "Ascending", sortFields: ["description"] };

export const Route = createFileRoute("/accounts/$id/rules")({
    validateSearch: (search: Record<string, unknown>) => validateGridSearch(search, rulesGridDefaults),
    component: Rules,
});

function Rules() {

    const account = useAccount();

    const fullTagsListQuery = useTags();
    const fullTagsList = fullTagsListQuery.data;

    const runTransactionTagRules = useRunRules();

    const runRules = () => {
        runTransactionTagRules.mutate({ path: { instrumentId: account.id } });
    };

    const { data: rules } = useRules(account?.id);

    const { search: { search = "" }, page: pageNumber, sortDirection, setPage, setSort, setFilter } = useGridSearch(rulesGridDefaults);
    const [pageSize, setPageSize] = useGridPageSize("rules-page-size", 20);
    const [searchTerm, setSearchTerm] = useSearchTerm(search, (term) => setFilter({ search: term || undefined }));

    const filteredRules = useMemo(() => {
        const term = search.toLocaleLowerCase();
        if (term === "") return rules ?? [];

        const matchingRules = rules?.filter(r => r.contains.toLocaleLowerCase().includes(term) || (r.description?.toLocaleLowerCase().includes(term) ?? false)) ?? [];
        const matchingTags = fullTagsList?.filter(t => t?.name.toLocaleLowerCase().includes(term)) ?? [];
        const matchingTagRules = rules?.filter(r => matchingRules.every(r2 => r2.id !== r.id) && r?.tags.some(t => matchingTags.some(t2 => t2.id === t.id))) ?? [];

        return matchingRules.concat(matchingTagRules);
    }, [rules, fullTagsList, search]);

    const pagedRules = useMemo(() =>
        [...filteredRules].sort(sortRules(sortDirection)).slice((pageNumber - 1) * pageSize, pageNumber * pageSize),
    [filteredRules, sortDirection, pageNumber, pageSize]);

    const numberOfPages = getNumberOfPages(filteredRules.length, pageSize);
    const totalRules = filteredRules.length;
    const pageChange = (_current: number, newPage: number) => setPage(newPage);

    usePageInRange(pageNumber, numberOfPages, !!rules);

    if (!account) return (null);

    return (
        <AccountPage title="Rules" breadcrumbs={[{ text: "Rules", route: `/accounts/${account.id}/rules` }]} actions={[{ id: "run", label: "Run Rules", icon: "check", onClick: runRules }]}>
            <Section>
                <SearchBox value={searchTerm} onChange={setSearchTerm} />
            </Section>
            <Table striped bordered={false} borderless className="section">
                <thead>
                    <tr>
                        <SortableTh className={`column-20 sortable ${sortDirection.toLowerCase()}`} onSort={() => setSort("description", changeSortDirection(sortDirection))} field="description" sortField="description" sortDirection={sortDirection}>When a transaction contains</SortableTh>
                        <th className="column-30">Apply tag(s)</th>
                        <th className="column-35">Notes</th>
                        <PaginationTh pageNumber={pageNumber} numberOfPages={numberOfPages} onChange={pageChange} />
                    </tr>
                </thead>
                <tbody>
                    <NewRule />
                    {!rules && <LoadingTableRows rows={pageSize} cols={4} />}
                    {rules && pagedRules.map((r) => <RuleRow key={r.id} accountId={account.id} rule={r} />)}
                </tbody>
                <tfoot>
                    <tr>
                        <td colSpan={1} className="page-totals">Page {pageNumber} of {numberOfPages} ({totalRules} tags)</td>
                        <td colSpan={3}>
                            <PaginationControls>
                                <PageSize value={pageSize} onChange={setPageSize} />
                                <Pagination pageNumber={pageNumber} numberOfPages={numberOfPages} onChange={pageChange} />
                            </PaginationControls>
                        </td>
                    </tr>
                </tfoot>
            </Table>
        </AccountPage>
    );
}
