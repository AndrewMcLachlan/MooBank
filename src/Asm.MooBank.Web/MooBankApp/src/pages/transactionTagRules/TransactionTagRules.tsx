import "./TransactionTagRules.scss";

import React, { useState } from "react";
import { Table } from "react-bootstrap";
//import Select from "react-select";

import { ClickableIcon } from "@andrewmclachlan/mooapp";
import { TagPanel } from "components";

import { TransactionTag, TransactionTagRule } from "models";
import { useParams } from "react-router-dom";

import { TransactionTagRuleRow } from "./TransactionTagRuleRow";
import { useAccount, useCreateRule, useCreateTag, useRules, useRunRules, useTags } from "services";
import { Page } from "layouts";

export const TransactionTagRules: React.FC = () => {

    const { accountId } = useParams<{accountId: string}>();

    const rulesQuery = useRules(accountId!);
    const { data } = rulesQuery;

    const { newRule, fullTagsList, addTag, createTag, removeTag, nameChange, createRule, runRules, keyUp } = useComponentState(accountId!);

    const account = useAccount(accountId!);

    if (!account.data) return (null);

    return (
        <Page title="Transaction Tag Rules">
            <Page.Header goBack title="Transaction Tag Rules" menuItems={[{ text: "Run Rules Now", onClick: runRules }]}
                breadcrumbs={[[account.data.name, `/accounts/${accountId}`], ["Tag Rules", `/accounts/${accountId}/tag-rules`]]} />
            <Page.Content>
                <Table striped bordered={false} borderless className="transaction-tag-rules">
                    <thead>
                        <tr>
                            <th className="column-20">When a transaction contains</th>
                            <th>Apply tag(s)</th>
                            <th className="column-5"></th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr>
                            <td><input type="text" className="form-control" placeholder="Description contains..." value={newRule.contains} onChange={nameChange} /></td>
                            <TagPanel as="td" selectedItems={newRule.tags} items={fullTagsList} labelField={t => t.name} valueField={t => t.id.toString()} onAdd={addTag} onCreate={createTag} onRemove={removeTag} allowCreate={false} alwaysShowEditPanel={true} onKeyUp={keyUp} />
                            <td className="row-action"><span onClick={createRule}><ClickableIcon icon="check-circle" title="Save" size="xl" /></span></td>
                        </tr>
                        {data?.rules && data.rules.map((r) => <TransactionTagRuleRow key={r.id} accountId={accountId!} rule={r} />)}
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
        createRule,
        keyUp,

        runRules,
    };
}