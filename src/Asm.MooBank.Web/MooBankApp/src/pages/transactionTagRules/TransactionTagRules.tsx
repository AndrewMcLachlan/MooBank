import "./TransactionTagRules.scss";

import React, { useState } from "react";
import { Table } from "react-bootstrap";
//import Select from "react-select";

import { ClickableIcon, TagPanel } from "@andrewmclachlan/mooapp";

import { TransactionTag, TransactionTagRule } from "../../models";
import { useParams } from "react-router-dom";

import { TransactionTagRuleRow } from "./TransactionTagRuleRow";
import { useAccount, useCreateRule, useCreateTag, useRules, useRunRules, useTags } from "../../services";
import { Page } from "../../layouts";

export const TransactionTagRules: React.FC = () => {


    const { accountId } = useParams<any>();

    const rulesQuery = useRules(accountId);
    const { data } = rulesQuery;

    const { newRule, fullTagsList, addTag, createTag, removeTag, nameChange, createRule, runRules } = useComponentState(accountId);

    const account = useAccount(accountId);

    if (!account.data) return (null);

    return (
        <Page title="Transaction Tag Rules">
            <Page.Header title="Transaction Tag Rules" menuItems={[{ text: "Run Rules Now", onClick: runRules }]}
                breadcrumbs={[[account.data.name, `/accounts/${accountId}`], ["Tag Rules", `/accounts/${accountId}/tag-rules`]]} />
            <Page.Content>
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
                            <td><input type="text" className="form-control" placeholder="Transaction description contains..." value={newRule.contains} onChange={nameChange} /></td>
                            <TagPanel as="td" selectedItems={newRule.tags} allItems={fullTagsList} textField="name" valueField="id" onAdd={addTag} onCreate={createTag} onRemove={removeTag} allowCreate={false} alwaysShowEditPanel={true} />
                            {/*<Select defaultOptions={fullTagsList} getOptionLabel={(t:TransactionTag) => t.name } getOptionValue={(t:TransactionTag) => t.id.toString() } hideSelectedOptions={true} isOptionSelected={t => } onChange />*/}
                            <td><span onClick={createRule}><ClickableIcon icon="check-circle" title="Save" /></span></td>
                        </tr>
                        {data?.rules && data.rules.map((r) => <TransactionTagRuleRow key={r.id} accountId={accountId} rule={r} />)}
                    </tbody>
                </Table>
            </Page.Content>
        </Page>
    );
}

TransactionTagRules.displayName = "TransactionTagRules";

const useComponentState = (accountId: string) => {

    const blankRule = { id: 0, contains: "", tags: [] } as TransactionTagRule;

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