import React, { useState, useRef, useEffect } from "react";
import { useDispatch, useSelector } from "react-redux";
import { bindActionCreators } from "redux";

import { TransactionTagRule, TransactionTag } from "../models";
import { actionCreators } from "../store/TransactionTags";
import { State } from "../store/state";
import { TagPanel } from "./TagPanel";

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
        </tr>
    );
}

function useTransactionTagRuleRowEvents(props: TransactionTagRuleRowProps) {

    const dispatch = useDispatch();
    const [tags, setTags] = useState(props.rule.tags);

    useEffect(() => {
        setTags(props.rule.tags);
    }, [props.rule.tags]);

    bindActionCreators(actionCreators, dispatch);

    const createTag = (name: string) => {
        dispatch(actionCreators.createTagAndAdd(props.rule.id, name));
    }

    const addTag = (tag: TransactionTag) => {

        if (!tag.id) return;

        dispatch(actionCreators.addTransactionTag(props.rule.id, tag.id));
        setTags(tags.concat([tag]));
    }

    const removeTag = (tag: TransactionTag) => {

        if (!tag.id) return;

        dispatch(actionCreators.removeTransactionTag(props.rule.id, tag.id));
        setTags(tags.filter((t) => t.id !== tag.id));
    }

    return {
        createTag,
        addTag,
        removeTag,
        tags,
    };
}

export interface TransactionTagRuleRowProps {
    rule: TransactionTagRule;
}