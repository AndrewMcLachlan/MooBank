import "./TransactionTagRules.scss";

import React, { useEffect, useState } from "react";
import { Col, Row, Table } from "react-bootstrap";

import { ClickableIcon } from "@andrewmclachlan/mooapp";
import { Pagination, TransactionTagPanel } from "components";

import { TransactionTag, TransactionTagRule, sortRules, sortTags } from "models";
import { useParams } from "react-router-dom";

import { TransactionTagRuleRow } from "./TransactionTagRuleRow";
import { useAccount, useCreateRule, useCreateTag, useRules, useRunRules, useTags } from "services";
import { Page } from "layouts";
import { getNumberOfPages } from "helpers/paging";
import { sortDirection } from "store/state";
import { changeSortDirection } from "helpers/sorting";
import { SearchBox } from "components/SearchBox";

export const TransactionTagRules: React.FC = () => {

    const { accountId } = useParams<{ accountId: string }>();

    const {data: rules} = useRules(accountId!);

    const [filteredRules, setFilteredRules] = useState<TransactionTagRule[]>([]);
    const [pagedRules, setPagedRules] = useState<TransactionTagRule[]>([]);
    const [pageNumber, setPageNumber] = useState<number>(1);
    const [pageSize, setPageSize] = useState<number>(20);
    const [sortDirection, setSortDirection] = useState<sortDirection>("Ascending");
    const [search, setSearch] = useState("");

    const numberOfPages = getNumberOfPages(filteredRules.length, pageSize);
    const totalTags = filteredRules.length;
    const pageChange = (_current: number, newPage: number) => setPageNumber(newPage);
    
    useEffect(() => {
        setFilteredRules(rules?.rules.filter(r => r.contains.toLocaleLowerCase().includes(search.toLocaleLowerCase())) ?? []);
    }, [rules, search]);

    useEffect(() => {
        setPagedRules(filteredRules.sort(sortRules(sortDirection)).slice((pageNumber - 1) * pageSize, ((pageNumber - 1) * pageSize) + pageSize));
    }, [filteredRules, sortDirection, pageNumber]);



    const { newRule, fullTagsList, addTag, createTag, removeTag, nameChange, descriptionChange, createRule, runRules, keyUp } = useComponentState(accountId!);

    const account = useAccount(accountId!);

    if (!account.data) return (null);

    return (
        <Page title="Transaction Tag Rules">
            <Page.Header goBack title="Transaction Tag Rules" menuItems={[{ text: "Run Rules Now", onClick: runRules }]}
                breadcrumbs={[[account.data.name, `/accounts/${accountId}`], ["Tag Rules", `/accounts/${accountId}/tag-rules`]]} />
            <Page.Content>
                <Row>
                    <Col xl={6}>
                        <SearchBox value={search} onChange={(v) => setSearch(v)} />
                    </Col>
                </Row>
                <Table striped bordered={false} borderless className="transaction-tag-rules">
                    <thead>
                        <tr>
                            <th className={`column-20 sortable ${sortDirection.toLowerCase()}`} onClick={() => setSortDirection(changeSortDirection(sortDirection))}>When a transaction contains</th>
                            <th className="column-30">Apply tag(s)</th>
                            <th className="column-35">Description</th>
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
                        {pagedRules.map((r) => <TransactionTagRuleRow key={r.id} accountId={accountId!} rule={r} />)}
                    </tbody>
                    <tfoot>
                        <tr>
                            <td colSpan={1} className="page-totals">Page {pageNumber} of {numberOfPages} ({totalTags} tags)</td>
                            <td colSpan={3}>
                                <Pagination pageNumber={pageNumber} numberOfPages={numberOfPages} onChange={pageChange} />
                            </td>
                        </tr>
                    </tfoot>
                </Table>
            </Page.Content>
        </Page>
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

    const addTag = (tag: TransactionTag) => {

        if (!tag.id) return;

        newRule.tags.push(tag);
        setNewRule(newRule);
    }

    const removeTag = (tag: TransactionTag) => {

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