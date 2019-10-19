import React, { useState, useRef, useEffect } from "react";
import { useDispatch, useSelector } from "react-redux";
import { bindActionCreators } from "redux";

import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";

import { TransactionTagRule, TransactionTag } from "../../models";
import { actionCreators } from "../../store/TransactionTagRules";
import { State } from "../../store/state";
import { TagPanel } from "../../components/TagPanel";
import { ClickableIcon } from "components/ClickableIcon";

export const TransactionTagRuleRow: React.FC<TransactionTagRuleRowProps> = (props) => {

    const transactionRow = useTransactionTagRuleRowEvents(props);

    const fullTagsList = useSelector((state: State) => state.transactionTags.tags);

    const [tagsList, setTagsList] = useState([]);

    useEffect(() => {
        setTagsList(fullTagsList.filter((t) => !transactionRow.tags.some((tt) => t.id === tt.id)));
    }, [transactionRow.tags, fullTagsList]);

    return (
        <tr>
            <td>{props.rule.contains}</td>
            <TagPanel as="td" selectedItems={transactionRow.tags} allItems={tagsList} textField="name" valueField="id" onAdd={transactionRow.addTag} onRemove={transactionRow.removeTag} onCreate={transactionRow.createTag} allowCreate={true} />
            <td><span onClick={transactionRow.deleteRule}><ClickableIcon icon="trash-alt" title="Delete" /></span></td>
        </tr>
    );
}

TransactionTagRuleRow.displayName = "TransactionTagRuleRow";

function useTransactionTagRuleRowEvents(props: TransactionTagRuleRowProps) {

    const dispatch = useDispatch();
    const [tags, setTags] = useState(props.rule.tags);

    useEffect(() => {
        setTags(props.rule.tags);
    }, [props.rule.tags]);

    bindActionCreators(actionCreators, dispatch);

    const deleteRule = () => {
        dispatch(actionCreators.deleteRule(props.rule));
    }

    const createTag = (name: string) => {
        dispatch(actionCreators.createTagAndAdd(props.rule.id, name));
    }

    const addTag = (tag: TransactionTag) => {

        if (!tag.id) return;

        dispatch(actionCreators.addTransactionTag(props.rule.id, tag.id));
        setTags([ ...tags, tag]);
    }

    const removeTag = (tag: TransactionTag) => {

        if (!tag.id) return;

        dispatch(actionCreators.removeTransactionTag(props.rule.id, tag.id));
        setTags(tags.filter((t) => t.id !== tag.id));
    }

    return {
        deleteRule,
        createTag,
        addTag,
        removeTag,
        tags,
    };
}

export interface TransactionTagRuleRowProps {
    rule: TransactionTagRule;
}