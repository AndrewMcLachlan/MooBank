import "./TransactionTagRules.scss";

import React, { useEffect, useState } from "react";
import { Table } from "react-bootstrap";
import { useDispatch, useSelector } from "react-redux";
import { bindActionCreators } from "redux";

import { State } from "../../store/state";
import { actionCreators } from "../../store/TransactionTagRules";
import { actionCreators as tagActionCreators } from "../../store/TransactionTags";

import { TagPanel } from "../../components";

import { TransactionTag, TransactionTagRule } from "models";
import { RouteComponentProps } from "react-router";
import { useSelectedAccount } from "hooks";

import { TransactionTagRuleRow} from "./TransactionTagRuleRow";
import { ClickableIcon } from "../../components/ClickableIcon";

export const TransactionTagRules: React.FC<TransactionTagRuleProps> = (props) => {

    const { newRule, fullTagsList, addTag, createTag, removeTag, nameChange, createRule } = useComponentState(props);

    const rules = useSelector((state: State) => state.transactionTagRules.rules);

    return (
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
                    <td><span onClick={createRule}><ClickableIcon icon="check-circle" title="Save" /></span></td>
                </tr>
                {rules && rules.map((r) => <TransactionTagRuleRow key={r.id} rule={r} />)}
            </tbody>
        </Table>

    );
}

TransactionTagRules.displayName = "TransactionTagRules";

const useComponentState = (props:TransactionTagRuleProps) => {

    var dispatch = useDispatch();

    const blankRule = {id: 0, contains: "", tags: []} as TransactionTagRule;

    const accountId = props.match.params.id;
    const account = useSelectedAccount(accountId);

    const fullTagsList = useSelector((state: State) => state.transactionTags.tags);

    const [newRule, setNewRule] = useState(blankRule); 
    const [tagsList, setTagsList] = useState([]);

    bindActionCreators(actionCreators, dispatch);
    bindActionCreators(tagActionCreators, dispatch);

    useEffect(() => {
        dispatch(actionCreators.requestRules());
    }, [account, dispatch]);

    useEffect(() => {
        setTagsList(fullTagsList.filter((t) => !newRule.tags.some((tt) => t.id === tt.id)));
    }, [newRule.tags, fullTagsList]);

    const createRule = () => {
        dispatch(actionCreators.createRule(newRule));

        setNewRule(blankRule);
    }

    const nameChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        setNewRule({...newRule, contains: e.currentTarget.value});
    }

    const createTag = (name: string) => {
        dispatch(tagActionCreators.createTag(name));
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
    }

    return {
        newRule,
        fullTagsList,
        
        createTag,
        addTag,
        removeTag,

        nameChange,
        createRule,
    };
}

export interface TransactionTagRuleProps extends RouteComponentProps<{ id: string }> {

}
