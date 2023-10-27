import React, { useState, useEffect } from "react";

import { Rule, Tag } from "models";
import { ClickableIcon, EditColumn } from "@andrewmclachlan/mooapp";
import { TagPanel } from "components";
import { useCreateTag, useTags } from "services/TagService";
import { useAddRuleTag, useDeleteRule, useRemoveRuleTag, useUpdateRule } from "services";

export const RuleRow: React.FC<RuleRowProps> = (props) => {

    const transactionRow = useRuleRowEvents(props);

    const fullTagsListQuery = useTags();
    const fullTagsList = fullTagsListQuery.data ?? [];

    const [tagsList, setTagsList] = useState([]);

    useEffect(() => {
        setTagsList(fullTagsList.filter((t) => !transactionRow.tags.some((tt) => t.id === tt.id)));
    }, [transactionRow.tags, fullTagsList]);

    return (
        <tr>
            <EditColumn value={props.rule.contains} onChange={(value) => transactionRow.updateRule({...props.rule, contains: value })} />
            <TagPanel as="td" selectedItems={transactionRow.tags} items={tagsList}  onAdd={transactionRow.addTag} onRemove={transactionRow.removeTag} onCreate={transactionRow.createTag} allowCreate={true} />
            <EditColumn value={props.rule.description} onChange={(value) => transactionRow.updateRule({...props.rule, description: value })} />
            <td className="row-action"><span onClick={transactionRow.deleteRule}><ClickableIcon icon="trash-alt" title="Delete" /></span></td>
        </tr>
    );
}

RuleRow.displayName = "RuleRow";

function useRuleRowEvents(props: RuleRowProps) {

    const updateTransctionTagRule = useUpdateRule();

    const createTransactionTag = useCreateTag();
    const addTransactionTagRuleTag = useAddRuleTag();
    const removeTransactionTagRuleTag = useRemoveRuleTag();
    
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

    const updateRule = (rule: Rule) => {
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

export interface RuleRowProps {
    rule: Rule;
    accountId: string;
}