import React, { useEffect, useState } from "react";
import { Table } from "react-bootstrap";

import { IconButton, Pagination, SearchBox, Section, SortDirection, changeSortDirection, getNumberOfPages } from "@andrewmclachlan/mooapp";
import { AccountPage, useAccount } from "components";

import { Rule, sortRules } from "models";

import { useRules, useRunRules, useTags } from "services";
import { NewRule } from "./NewRule";
import { RuleRow } from "./RuleRow";

export const Rules: React.FC = () => {

    const account = useAccount();

    const fullTagsListQuery = useTags();
    const fullTagsList = fullTagsListQuery.data ?? [];

    const runTransactionTagRules = useRunRules();

    const runRules = () => {
        runTransactionTagRules.mutate({ accountId: account.id });
    };

    const { data: rules } = useRules(account?.id);

    const [filteredRules, setFilteredRules] = useState<Rule[]>([]);
    const [pagedRules, setPagedRules] = useState<Rule[]>([]);
    const [pageNumber, setPageNumber] = useState<number>(1);
    const [pageSize, _setPageSize] = useState<number>(20);
    const [sortDirection, setSortDirection] = useState<SortDirection>("Ascending");
    const [search, setSearch] = useState("");

    const numberOfPages = getNumberOfPages(filteredRules.length, pageSize);
    const totalRules = filteredRules.length;
    const pageChange = (_current: number, newPage: number) => setPageNumber(newPage);

    useEffect(() => {
        setPageNumber(1);
        const searchTerm = search.toLocaleLowerCase();
        if (searchTerm === "") {
            setFilteredRules(rules ?? []);
            return;
        }

        const matchingRules = rules?.filter(r => r.contains.toLocaleLowerCase().includes(searchTerm) || (r.description?.toLocaleLowerCase().includes(searchTerm) ?? false)) ?? [];
        const matchingTags = fullTagsList?.filter(t => t?.name.toLocaleLowerCase().includes(searchTerm)) ?? [];
        const matchingTagRules = rules?.filter(r => matchingRules.every(r2 => r2.id !== r.id) && r?.tags.some(t => matchingTags.some(t2 => t2.id === t.id)));

        setFilteredRules(matchingRules.concat(matchingTagRules));
    }, [JSON.stringify(rules), search]);

    useEffect(() => {
        setPagedRules(filteredRules.sort(sortRules(sortDirection)).slice((pageNumber - 1) * pageSize, ((pageNumber - 1) * pageSize) + pageSize));
    }, [JSON.stringify(filteredRules), sortDirection, pageNumber]);

    if (!account) return (null);

    return (
        <AccountPage title="Rules" breadcrumbs={[{ text: "Rules", route: `/accounts/${account.id}/rules` }]} actions={[<IconButton key="run" icon="check" onClick={runRules}>Run Rules</IconButton>]}>
            <Section>
                <SearchBox value={search} onChange={(v: string) => setSearch(v)} />
            </Section>
            <Table striped bordered={false} borderless className="section">
                <thead>
                    <tr>
                        <th className={`column-20 sortable ${sortDirection.toLowerCase()}`} onClick={() => setSortDirection(changeSortDirection(sortDirection))}>When a transaction contains</th>
                        <th className="column-30">Apply tag(s)</th>
                        <th className="column-35">Notes</th>
                        <th className="column-5"></th>
                    </tr>
                </thead>
                <tbody>
                    <NewRule />
                    {pagedRules.map((r) => <RuleRow key={r.id} accountId={account.id} rule={r} />)}
                </tbody>
                <tfoot>
                    <tr>
                        <td colSpan={1} className="page-totals">Page {pageNumber} of {numberOfPages} ({totalRules} tags)</td>
                        <td colSpan={3}>
                            <Pagination pageNumber={pageNumber} numberOfPages={numberOfPages} onChange={pageChange} />
                        </td>
                    </tr>
                </tfoot>
            </Table>
        </AccountPage>
    );
}
