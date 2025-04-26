import React, { useState } from "react";

import { TagPanel, useAccount } from "components";

import { Tag, emptyRule } from "models";

import { SaveIcon } from "@andrewmclachlan/mooapp";
import { useCreateRule, useCreateTag, useTags } from "services";

export const NewRule: React.FC = () => {

    const account = useAccount();

    const [newRule, setNewRule] = useState(emptyRule);

    const fullTagsListQuery = useTags();
    const fullTagsList = fullTagsListQuery.data ?? [];

    const createTag = useCreateTag();
    const createRule = useCreateRule();

    const save = () => {
        createRule.mutate([{ accountId: account.id }, newRule]);
        setNewRule(emptyRule);
    }

    const nameChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        setNewRule({ ...newRule, contains: e.currentTarget.value });
    }

    const descriptionChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        setNewRule({ ...newRule, description: e.currentTarget.value });
    }

    const tagCreateHandler = (name: string) => {
        createTag.mutate({ name });
    }

    const addTag = (tag: Tag) => {

        if (!tag.id) return;

        setNewRule({...newRule, tags: [...newRule.tags, tag]});
    }

    const removeTag = (tag: Tag) => {

        if (!tag.id) return;

        setNewRule({ ...newRule, tags: newRule.tags.filter((t) => t.id !== tag.id) });
    };

    const keyUp: React.KeyboardEventHandler<HTMLElement> = (e) => {
        if (e.key === "Enter") {
            save();
        }
    }

    return (
        <tr>
            <td><input type="text" className="form-control" placeholder="Description contains..." value={newRule.contains} onChange={nameChange} /></td>
            <TagPanel as="td" selectedItems={newRule.tags} items={fullTagsList} onAdd={addTag} onCreate={tagCreateHandler} onRemove={removeTag} allowCreate={false} alwaysShowEditPanel={true} onKeyUp={keyUp} />
            <td><input type="text" className="form-control" placeholder="Notes..." value={newRule.description} onChange={descriptionChange} onKeyUp={keyUp} /></td>
            <td className="row-action column-5"><SaveIcon onClick={save} /></td>
        </tr>
    );

}
