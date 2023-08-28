import "./TransactionTagRules.scss";

import React, { useEffect, useState } from "react";
import { Button, Col, Row, Table } from "react-bootstrap";

import { changeSortDirection, ClickableIcon, getNumberOfPages, SearchBox, SortDirection, Pagination, useIdParams, Section } from "@andrewmclachlan/mooapp";
import { AccountPage, TransactionTagPanel, useAccount } from "components";

import { Tag, TransactionTagRule, sortRules } from "models";

import { TransactionTagRuleRow } from "./TransactionTagRuleRow";
import { useCreateRule, useCreateTag, useRules, useRunRules, useTags } from "services";
import { Page } from "layouts";


export const TransactionTagRules: React.FC = () => {

    const id = useIdParams();
    const account = useAccount();

    const { newRule, fullTagsList, addTag, createTag, removeTag, nameChange, descriptionChange, createRule, runRules, keyUp } = useComponentState(id);

    const { data: rules } = useRules(id);

    const [filteredRules, setFilteredRules] = useState<TransactionTagRule[]>([]);
    const [pagedRules, setPagedRules] = useState<TransactionTagRule[]>([]);
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
            setFilteredRules(rules?.rules ?? []);
            return;
        }

        const matchingRules = rules?.rules.filter(r => r.contains.toLocaleLowerCase().includes(searchTerm) || (r.description?.toLocaleLowerCase().includes(searchTerm) ?? false)) ?? [];
        const matchingTags = fullTagsList?.filter(t => t?.name.toLocaleLowerCase().includes(searchTerm)) ?? [];
        const matchingTagRules = rules?.rules.filter(r => matchingRules.every(r2 => r2.id !== r.id) && r?.tags.some(t => matchingTags.some(t2 => t2.id === t.id)));

        setFilteredRules(matchingRules.concat(matchingTagRules));
    }, [rules, search]);

    useEffect(() => {
        setPagedRules(filteredRules.sort(sortRules(sortDirection)).slice((pageNumber - 1) * pageSize, ((pageNumber - 1) * pageSize) + pageSize));
    }, [filteredRules, sortDirection, pageNumber]);

    if (!account) return (null);

    return (
        <AccountPage title="Transaction Tag Rules" breadcrumbs={[{ text: "Tag Rules", route: `/accounts/${id}/tag-rules` }]}>
            <div className="section-group">
                <Section className="col-xl-4">
                    <SearchBox value={search} onChange={(v: string) => setSearch(v)} />
                </Section>
                <Section>
                    <Button onClick={runRules}>Run Rules</Button>
                </Section>
            </div>
            <Table striped bordered={false} borderless className="section transaction-tag-rules">
                <thead>
                    <tr>
                        <th className={`column-20 sortable ${sortDirection.toLowerCase()}`} onClick={() => setSortDirection(changeSortDirection(sortDirection))}>When a transaction contains</th>
                        <th className="column-30">Apply tag(s)</th>
                        <th className="column-35">Notes</th>
                        <th className="column-5"></th>
                    </tr>
                </thead>
                <tbody>
                    <tr>
                        <td><input type="text" className="form-control" placeholder="Description contains..." value={newRule.contains} onChange={nameChange} /></td>
                        <TransactionTagPanel as="td" selectedItems={newRule.tags} items={fullTagsList} onAdd={addTag} onCreate={createTag} onRemove={removeTag} allowCreate={false} alwaysShowEditPanel={true} onKeyUp={keyUp} />
                        <td><input type="text" className="form-control" placeholder="Notes..." value={newRule.description} onChange={descriptionChange} /></td>
                        <td className="row-action"><span onClick={createRule}><ClickableIcon icon="check-circle" title="Save" size="xl" /></span></td>
                    </tr>
                    {pagedRules.map((r) => <TransactionTagRuleRow key={r.id} accountId={id} rule={r} />)}
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

TransactionTagRules.displayName = "TransactionTagRules";

const useComponentState = (accountId: string) => {

    const blankRule = { id: 0, contains: "", description: "", tags: [] } as TransactionTagRule;

    const fullTagsListQuery = useTags();
    const fullTagsList = fullTagsListQuery.data ?? [];

    const [newRule, setNewRule] = useState(blankRule);

    const createTransactionTag = useCreateTag();
    const createTransactionTagRule = useCreateRule();
    const runTransactionTagRules = useRunRules();

    const createRule = () => {
        createTransactionTagRule.mutate([{ accountId }, newRule]);
        setNewRule(blankRule);
    }

    const nameChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        setNewRule({ ...newRule, contains: e.currentTarget.value });
    }

    const descriptionChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        setNewRule({ ...newRule, description: e.currentTarget.value });
    }

    const createTag = (name: string) => {
        createTransactionTag.mutate({ name });
    }

    const addTag = (tag: Tag) => {

        if (!tag.id) return;

        newRule.tags.push(tag);
        setNewRule(newRule);
    }

    const removeTag = (tag: Tag) => {

        if (!tag.id) return;

        newRule.tags = newRule.tags.filter((t) => t.id !== tag.id);
        setNewRule(newRule);
    };

    const runRules = () => {
        runTransactionTagRules.mutate({ accountId });
    };

    const keyUp: React.KeyboardEventHandler<HTMLTableCellElement> = (e) => {
        if (e.key === "Enter") {
            createRule();
        }
    }
    return {
        newRule,
        fullTagsList,

        createTag,
        addTag,
        removeTag,

        nameChange,
        descriptionChange,
        createRule,
        keyUp,

        runRules,
    };
}