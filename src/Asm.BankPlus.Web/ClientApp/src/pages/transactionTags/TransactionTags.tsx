import "./TransactionTags.scss";

import React, { useEffect, useState } from "react";
import { Table } from "react-bootstrap";
import { useDispatch, useSelector } from "react-redux";
import { bindActionCreators } from "redux";

import { actionCreators } from "../../store/TransactionTags";
import { State } from "../../store/state";
import { TransactionTagRow } from "./TransactionTagRow";

import { TagPanel } from "../../components";
import { TransactionTag } from "../../models";
import { ClickableIcon } from "../../components/ClickableIcon";
import { usePageTitle } from "../../hooks";

export const TransactionTags: React.FC = () => {

    usePageTitle("Tags");

    const { newTag, fullTagsList, tagsList, addTag, createTag, removeTag, nameChange } = useComponentState();

    return (
        <>
        <h1>Transaction Tags</h1>
        <Table striped bordered={false} borderless className="transaction-tags">
            <thead>
                <tr>
                    <th>Name</th>
                    <th>Tags</th>
                </tr>
            </thead>
            <tbody>
                <tr>
                    <td><input type="text" placeholder="Tag name" value={newTag.name} onChange={nameChange} /></td>
                    <TagPanel as="td" selectedItems={newTag.tags} allItems={tagsList} textField="name" valueField="id" onAdd={addTag} onCreate={createTag} onRemove={removeTag} allowCreate={false} alwaysShowEditPanel={true} />
                    <td><span onClick={createTag}><ClickableIcon icon="check-circle" title="Save" /></span></td>
                </tr>
                {fullTagsList && fullTagsList.map((t) => <TransactionTagRow key={t.id} tag={t} />)}
            </tbody>
        </Table>
        </>
    );
}

const useComponentState = () => {

    var dispatch = useDispatch();

    const blankTag = {id: 0, name: "", tags: []} as TransactionTag;

    const fullTagsList = useSelector((state: State) => state.transactionTags.tags);

    const [newTag, setNewTag] = useState(blankTag); 
    const [tagsList, setTagsList] = useState([]);

    bindActionCreators(actionCreators, dispatch);

    useEffect(() => {
        dispatch(actionCreators.requestTags());
    }, [dispatch]);

    useEffect(() => {
        setTagsList(fullTagsList.filter((t) => !newTag.tags.some((tt) => t.id === tt.id)));
    }, [newTag.tags, fullTagsList]);

    const createTag = () => {
        dispatch(actionCreators.createTag(newTag));
        setNewTag(blankTag);
    }

    const createSubTag = (name: string) => {
        dispatch(actionCreators.createTag(name));
    }

    const nameChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        setNewTag({...newTag, name: e.currentTarget.value});
    }

    const addTag = (tag: TransactionTag) => {

        if (!tag.id) return;

        //dispatch(actionCreators.addTransactionTag(props.transaction.id, tag.id));
        //setTags(tags.concat([tag]));
        newTag.tags.push(tag);
        setNewTag(newTag);
    }

    const removeTag = (tag: TransactionTag) => {

        if (!tag.id) return;

        newTag.tags = newTag.tags.filter((t) => t.id !== tag.id);
        setNewTag(newTag);
    }

    return {
        newTag,
        fullTagsList,
        tagsList,
        
        createTag,
        createSubTag,
        addTag,
        removeTag,

        nameChange,
    };
}