import React from "react";
import { Tooltip, useUpdatingState } from "@andrewmclachlan/moo-ds";
import type { Tag } from "api/types.gen";
import { Button, Input } from "@andrewmclachlan/moo-ds";
import { ResponsiveDialog } from "components/ResponsiveDialog";
import { useUpdateTag } from "../-hooks/useUpdateTag";
import { TransactionTagTransactionTagPanel } from "./TagTagPanel";
import { onKeyLeave } from "utils/onKeyLeave";
import { ColourPicker } from "components/ColourPicker";

export const TransactionTagDetails: React.FC<TransactionTagDetailsProps> = (props) => {

    const [tag, setTag] = useUpdatingState(props.tag);
    const [name, setName] = useUpdatingState(props.tag.name);

    const updateTag = useUpdateTag();

    const updateExcludeFromReporting = (excludeFromReporting: boolean) => save({ ...tag, settings: { ...tag.settings, excludeFromReporting } });
    const updateAllowSmoothing = (allowSmoothing: boolean) => save({ ...tag, settings: { ...tag.settings, applySmoothing: allowSmoothing } });
    const updateBudgetCategory = (budgetCategory: boolean) => save({ ...tag, settings: { ...tag.settings, budgetCategory } });
    const updateName = (name: string) => save({ ...tag, name });

    const save = (newTag: Tag) => {
        updateTag.mutate(newTag);
        setTag(newTag);
    }

    return (
        <ResponsiveDialog show={props.show} onHide={props.onHide} size="lg">
            <ResponsiveDialog.Header closeButton>
                <ResponsiveDialog.Title>Tag</ResponsiveDialog.Title>
            </ResponsiveDialog.Header>
            <ResponsiveDialog.Body>
                <section className="tag-details">
                    <label htmlFor="name">Name</label>
                    <Input id="name" placeholder="Name" type="text" value={name} onChange={(e) => setName(e.currentTarget.value)} onBlur={(e) => updateName(e.currentTarget.value)} onKeyUp={(e) => onKeyLeave(e, updateName)} />
                    <label htmlFor="colour">Colour</label>
                    <ColourPicker id="colour" value={(tag.colour as string) ?? null} onChange={(colour) => save({ ...tag, colour })} />
                    <label htmlFor="exclude">Exclude from Reporting</label>
                    <Input.Switch id="exclude" checked={tag.settings?.excludeFromReporting} onChange={(e) => updateExcludeFromReporting(e.currentTarget.checked)} />
                    <label htmlFor="smooth">Allow Smoothing<Tooltip id="smoothing">Provides an option to average non-monthly transactions in trend reports</Tooltip></label>
                    <Input.Switch id="smooth" checked={tag.settings?.applySmoothing} onChange={(e) => updateAllowSmoothing(e.currentTarget.checked)} />
                    <label htmlFor="budget-category">Budget Category<Tooltip id="budget-category-tip">When generating a budget, spending from child tags rolls up into this tag</Tooltip></label>
                    <Input.Switch id="budget-category" checked={tag.settings?.budgetCategory} onChange={(e) => updateBudgetCategory(e.currentTarget.checked)} />
                    <label htmlFor="tags">Tags</label>
                    <TransactionTagTransactionTagPanel as="div" id="tags" tag={tag} alwaysShowEditPanel />
                </section>
            </ResponsiveDialog.Body>
            <ResponsiveDialog.Footer>
                <Button variant="primary" onClick={props.onHide}>Close</Button>
            </ResponsiveDialog.Footer>
        </ResponsiveDialog>
    );
}

export interface TransactionTagDetailsProps {
    tag: Tag
    show: boolean;
    onHide: () => void;
}
