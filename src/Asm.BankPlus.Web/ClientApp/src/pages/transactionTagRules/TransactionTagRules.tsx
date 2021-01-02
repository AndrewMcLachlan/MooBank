import "./TransactionTagRules.scss";

import React, { useEffect, useState } from "react";
import { Table } from "react-bootstrap";
import Select from "react-select";

import { TagPanel, PageHeader } from "../../components";

import { TransactionTag, TransactionTagRule } from "../../models";
import { RouteComponentProps, useParams } from "react-router";
import { usePageTitle } from "../../hooks";

import { TransactionTagRuleRow } from "./TransactionTagRuleRow";
import { ClickableIcon } from "../../components/ClickableIcon";
import { useCreateRule, useCreateTag, useRules, useRunRules, useTags, useTransactions } from "../../services";

export const TransactionTagRules: React.FC = () => {

    usePageTitle("Transaction Tag Rules");

    const { accountId } = useParams<any>();

    const rulesQuery = useRules(accountId);
    const { data } = rulesQuery;

    console.debug(rulesQuery);

    const { newRule, fullTagsList, addTag, createTag, removeTag, nameChange, createRule, runRules } = useComponentState(accountId);

    return (
        <>
            <PageHeader title="Transaction Tag Rules" menuItems={[
                { text: "Run Rules Now", onClick: runRules }
            ]} />
            <Table striped bordered={false} borderless className="transaction-tag-rules">
                <thead>
                    <tr>
                        <th>When a transaction contains</th>
                        <th>Apply tag(s)</th>
                        <th></th>
                    </tr>
                </thead>
                <tbody>
                    <tr>
                        <td><input type="text" placeholder="Transaction description contains..." value={newRule.contains} onChange={nameChange} /></td>
                        <TagPanel as="td" selectedItems={newRule.tags} allItems={fullTagsList} textField="name" valueField="id" onAdd={addTag} onCreate={createTag} onRemove={removeTag} allowCreate={false} alwaysShowEditPanel={true} />
                        {/*<Select defaultOptions={fullTagsList} getOptionLabel={(t:TransactionTag) => t.name } getOptionValue={(t:TransactionTag) => t.id.toString() } hideSelectedOptions={true} isOptionSelected={t => } onChange />*/}
                        <td><span onClick={createRule}><ClickableIcon icon="check-circle" title="Save" /></span></td>
                    </tr>
                    {data?.rules && data.rules.map((r) => <TransactionTagRuleRow key={r.id} accountId={accountId} rule={r} />)}
                </tbody>
            </Table>
        </>
    );
}

TransactionTagRules.displayName = "TransactionTagRules";

const useComponentState = (accountId: string) => {

    const blankRule = { id: 0, contains: "", tags: [] } as TransactionTagRule;

    const fullTagsListQuery = useTags();
    const fullTagsList = fullTagsListQuery.data ?? [];

    const [newRule, setNewRule] = useState(blankRule);
    const [tagsList, setTagsList] = useState([]);

    const createTransactionTag = useCreateTag();
    const createTransactionTagRule = useCreateRule();
    const runTransactionTagRules = useRunRules();

    useEffect(() => {
        setTagsList(fullTagsList.filter((t) => !newRule.tags.some((tt) => t.id === tt.id)));
    }, [newRule.tags, fullTagsListQuery]);

    const createRule = () => {
        createTransactionTagRule.mutate([{ accountId }, newRule]);
        setNewRule(blankRule);
    }

    const nameChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        setNewRule({ ...newRule, contains: e.currentTarget.value });
    }

    const createTag = (name: string) => {
        createTransactionTag.mutate({ name });
    }

    const addTag = (tag: TransactionTag) => {

        if (!tag.id) return;

        //dispatch(actionCreators.addTransactionTag(props.transaction.id, tag.id));
        //setTags(tags.concat([tag]));
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

    return {
        newRule,
        fullTagsList,

        createTag,
        addTag,
        removeTag,

        nameChange,
        createRule,

        runRules,
    };
}