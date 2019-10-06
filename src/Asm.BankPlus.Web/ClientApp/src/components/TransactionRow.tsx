﻿import React, { useState, useRef, useEffect } from "react";
import moment from "moment";

import { Transaction, TransactionTag } from "../models";
import { Badge, Button } from "react-bootstrap";
import { CloseBadge, ComboBox } from "components";
import { useDispatch, useSelector } from "react-redux";
import { bindActionCreators } from "redux";
import { actionCreators } from "store/Transactions";
import { actionCreators as tagActionCreators } from "store/TransactionTags";
import { State } from "store/state";
import { useClickAway } from "hooks/clickAway";
import { TagPanel } from "./TagPanel";

export const TransactionRow: React.FC<TransactionRowProps> = (props) => {

    //const [editMode, setEditMode] = useState(false);
    const transactionRow = useTransactionRowEvents(props);

    const fullTagsList = useSelector((state: State) => state.transactionTags.tags);

    const [tagsList, setTagsList] = useState([]);

    useEffect(() => {
        setTagsList(fullTagsList.filter((t) => !transactionRow.tags.some((tt) => t.id === tt.id)));
    }, [transactionRow.tags]);

    return (
        <tr>
            <td>{moment(props.transaction.transactionTime).format("YYYY-MM-DD")}</td>
            <td>{props.transaction.description}</td>
            <td>{props.transaction.amount.toLocaleString("en-AU", { minimumFractionDigits: 2, maximumFractionDigits: 2 })}</td>
            <TagPanel as="td" selectedItems={transactionRow.tags} allItems={tagsList} textField="name" onAdd={transactionRow.addTag} onRemove={transactionRow.removeTag} onCreate={transactionRow.createTag} allowCreate={true} />
        </tr>
    );
}

function useTransactionRowEvents(props: TransactionRowProps) {

    const dispatch = useDispatch();
    const [tags, setTags] = useState(props.transaction.tags);

    bindActionCreators(actionCreators, dispatch);
    bindActionCreators(tagActionCreators, dispatch);

    const createTag = (name: string) => {
        alert("here");
        dispatch(actionCreators.createTagAndAdd(props.transaction.id, name));
    }

    const addTag = (tag: TransactionTag) => {

        if (!tag.id) return;

        dispatch(actionCreators.addTransactionTag(props.transaction.id, tag.id));
        setTags(tags.concat([tag]));
    }

    const removeTag = (tag: TransactionTag) => {
        dispatch(actionCreators.removeTransactionTag(props.transaction.id, tag.id));
        setTags(tags.filter((t) => t.id !== tag.id));
    }

    return {
        createTag,
        addTag,
        removeTag,
        tags,
    };
}

export interface TransactionRowProps {
    transaction: Transaction;
}