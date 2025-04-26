import React, { useEffect, useState } from "react";

import { DeleteIcon, EditColumn } from "@andrewmclachlan/mooapp";
import { TagPanel } from "components";
import { Rule, Tag } from "models";
import { useAddRuleTag, useDeleteRule, useRemoveRuleTag, useUpdateRule } from "services";
import { useCreateTag, useTags } from "services/TagService";

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
            <EditColumn value={props.rule.contains} onChange={target => transactionRow.updateRule({...props.rule, contains: target.value })} />
            <TagPanel as="td" selectedItems={transactionRow.tags} items={tagsList}  onAdd={transactionRow.addTag} onRemove={transactionRow.removeTag} onCreate={transactionRow.createTag} allowCreate={true} />
            <EditColumn value={props.rule.description} onChange={target => transactionRow.updateRule({...props.rule, description: target.value })} />
            <td className="row-action column-5"><span onClick={transactionRow.deleteRule}><DeleteIcon /></span></td>
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
        if (confirm("Are you sure you want to delete this rule?")) {
            deleteTransactionTagRule.mutate({accountId: props.accountId, ruleId: props.rule.id});
        }
    };

    const createTag = (name: string) => {
        createTransactionTag.mutate({ name }, {
            onSuccess: (data) => {
                addTransactionTagRuleTag.mutate({ instrumentId: props.accountId, ruleId: props.rule.id, tag: data});
            }
        });
    };

    const addTag = (tag: Tag) => {

        if (!tag.id) return;

        addTransactionTagRuleTag.mutate({ instrumentId: props.accountId, ruleId: props.rule.id, tag });
        setTags([ ...tags, tag]);
    };

    const removeTag = (tag: Tag) => {

        if (!tag.id) return;

        removeTransactionTagRuleTag.mutate({ instrumentId: props.accountId, ruleId: props.rule.id, tag });
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
