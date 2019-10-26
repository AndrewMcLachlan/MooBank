import React, { useState, useEffect } from "react";

import { TransactionTag } from "../../models";
import { useDispatch, useSelector } from "react-redux";
import { bindActionCreators } from "redux";
import { actionCreators } from "../../store/TransactionTags";
import { State } from "../../store/state";
import { TagPanel } from "../../components/TagPanel";
import { ClickableIcon } from "../../components/ClickableIcon";

export const TransactionTagRow: React.FC<TransactionTagRowProps> = (props) => {

    const transactionRow = useTransactionTagRowEvents(props);

    const fullTagsList = useSelector((state: State) => state.transactionTags.tags);

    const [tagsList, setTagsList] = useState([]);

    useEffect(() => {
        setTagsList(fullTagsList.filter((t) => t.id !== props.tag.id && !transactionRow.tags.some((tt) => t.id === tt.id)));
    }, [transactionRow.tags, fullTagsList]);

    return (
        <tr>
            <td>{props.tag.name}</td>
            <TagPanel as="td" selectedItems={transactionRow.tags} allItems={tagsList} textField="name" valueField="id" onAdd={transactionRow.addTag} onRemove={transactionRow.removeTag} onCreate={transactionRow.createTag} allowCreate={true} />
            <td><span onClick={transactionRow.deleteTag}><ClickableIcon icon="trash-alt" title="Delete" /></span></td>
        </tr>
    );
}

function useTransactionTagRowEvents(props: TransactionTagRowProps) {

    const dispatch = useDispatch();
    const [tags, setTags] = useState(props.tag.tags);

    useEffect(() => {
        setTags(props.tag.tags);
    }, [props.tag.tags]);

    bindActionCreators(actionCreators, dispatch);

    const createTag = (name: string) => {
        dispatch(actionCreators.createTagAndAdd(props.tag.id, name));
    }

    const deleteTag = () => {

        if (window.confirm("Deleting this tag will remove it from all rules and transactions. Are you sure?")) {
            dispatch(actionCreators.deleteTag(props.tag));
        }
    }

    const addTag = (tag: TransactionTag) => {

        if (!tag.id) return;

        dispatch(actionCreators.addTransactionTag(props.tag.id, tag.id));
        setTags(tags.concat([tag]));
    }

    const removeTag = (tag: TransactionTag) => {

        if (!tag.id) return;

        dispatch(actionCreators.removeTransactionTag(props.tag.id, tag.id));
        setTags(tags.filter((t) => t.id !== tag.id));
    }

    return {
        createTag,
        deleteTag,
        addTag,
        removeTag,
        tags,
    };
}

export interface TransactionTagRowProps {
    tag: TransactionTag;
}