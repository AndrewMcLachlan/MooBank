import React, { useState, useEffect } from "react";

import { TransactionTagRule, TransactionTag } from "../../models";
import { ClickableIcon, TagPanel } from "@andrewmclachlan/mooapp";
import { useCreateTag, useTags } from "../../services/TransactionTagService";
import { useAddTransactionTagRuleTag, useDeleteRule, useRemoveTransactionTagRuleTag } from "../../services";

export const TransactionTagRuleRow: React.FC<TransactionTagRuleRowProps> = (props) => {

    const transactionRow = useTransactionTagRuleRowEvents(props);

    const fullTagsListQuery = useTags();
    const fullTagsList = fullTagsListQuery.data ?? [];

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

    const createTransactionTag = useCreateTag();
    const addTransactionTagRuleTag = useAddTransactionTagRuleTag();
    const removeTransactionTagRuleTag = useRemoveTransactionTagRuleTag();
    
    const deleteTransactionTagRule = useDeleteRule();

    const [tags, setTags] = useState(props.rule.tags);

    useEffect(() => {
        setTags(props.rule.tags);
    }, [props.rule.tags]);

    const deleteRule = () => {
        deleteTransactionTagRule.mutate({accountId: props.accountId, ruleId: props.rule.id});
    };

    const createTag = (name: string) => {
        createTransactionTag.mutate({ name }, {
            onSuccess: (data) => {
                addTransactionTagRuleTag.mutate({ accountId: props.accountId, ruleId: props.rule.id, tag: data});
            }
        });
    };

    const addTag = (tag: TransactionTag) => {

        if (!tag.id) return;

        addTransactionTagRuleTag.mutate({ accountId: props.accountId, ruleId: props.rule.id, tag });
        setTags([ ...tags, tag]);
    };

    const removeTag = (tag: TransactionTag) => {

        if (!tag.id) return;

        removeTransactionTagRuleTag.mutate({ accountId: props.accountId, ruleId: props.rule.id, tag });
        setTags(tags.filter((t) => t.id !== tag.id));
    };

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
    accountId: string;
}