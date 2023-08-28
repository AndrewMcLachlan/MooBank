import React, { useState, useEffect } from "react";

import { TransactionTagRule, Tag } from "models";
import { ClickableIcon, EditColumn } from "@andrewmclachlan/mooapp";
import { TransactionTagPanel } from "components";
import { useCreateTag, useTags } from "services/TagService";
import { useAddTransactionTagRuleTag, useDeleteRule, useRemoveTransactionTagRuleTag, useUpdateRule } from "services";

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
            <EditColumn value={props.rule.contains} onChange={(value) => transactionRow.updateRule({...props.rule, contains: value })} />
            <TransactionTagPanel as="td" selectedItems={transactionRow.tags} items={tagsList}  onAdd={transactionRow.addTag} onRemove={transactionRow.removeTag} onCreate={transactionRow.createTag} allowCreate={true} />
            <EditColumn value={props.rule.description} onChange={(value) => transactionRow.updateRule({...props.rule, description: value })} />
            <td className="row-action"><span onClick={transactionRow.deleteRule}><ClickableIcon icon="trash-alt" title="Delete" /></span></td>
        </tr>
    );
}

TransactionTagRuleRow.displayName = "TransactionTagRuleRow";

function useTransactionTagRuleRowEvents(props: TransactionTagRuleRowProps) {

    const updateTransctionTagRule = useUpdateRule();

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

    const addTag = (tag: Tag) => {

        if (!tag.id) return;

        addTransactionTagRuleTag.mutate({ accountId: props.accountId, ruleId: props.rule.id, tag });
        setTags([ ...tags, tag]);
    };

    const removeTag = (tag: Tag) => {

        if (!tag.id) return;

        removeTransactionTagRuleTag.mutate({ accountId: props.accountId, ruleId: props.rule.id, tag });
        setTags(tags.filter((t) => t.id !== tag.id));
    };

    const updateRule = (rule: TransactionTagRule) => {
        updateTransctionTagRule.mutate([{accountId: props.accountId, id: props.rule.id, }, rule]);
    }

    return {
        deleteRule,
        createTag,
        addTag,
        removeTag,
        updateRule,
        tags,
    };
}

export interface TransactionTagRuleRowProps {
    rule: TransactionTagRule;
    accountId: string;
}