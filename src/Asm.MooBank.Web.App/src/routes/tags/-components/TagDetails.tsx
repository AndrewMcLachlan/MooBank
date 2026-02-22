import React from "react";
import { Tooltip, useUpdatingState } from "@andrewmclachlan/moo-ds";
import type { Tag } from "api/types.gen";
import { Button, Input, Modal } from "@andrewmclachlan/moo-ds";
import { useUpdateTag } from "../-hooks/useUpdateTag";
import { TransactionTagTransactionTagPanel } from "./TagTagPanel";
import { onKeyLeave } from "utils/onKeyLeave";

export const TransactionTagDetails: React.FC<TransactionTagDetailsProps> = (props) => {

    const [tag, setTag] = useUpdatingState(props.tag);
    const [name, setName] = useUpdatingState(props.tag.name);

    const updateTag = useUpdateTag();

    const updateExcludeFromReporting = (excludeFromReporting: boolean) => save({ ...tag, settings: { ...tag.settings, excludeFromReporting } });
    const updateAllowSmoothing = (allowSmoothing: boolean) => save({ ...tag, settings: { ...tag.settings, applySmoothing: allowSmoothing } });
    const updateName = (name: string) => save({ ...tag, name });

    const save = (newTag: Tag) => {
        updateTag.mutate(newTag);
        setTag(newTag);
    }

    return (
        <Modal show={props.show} onHide={props.onHide} size="lg">
            <Modal.Header closeButton>
                <Modal.Title>Tag</Modal.Title>
            </Modal.Header>
            <Modal.Body>
                <section className="tag-details">
                    <label htmlFor="name">Name</label>
                    <Input id="name" placeholder="Name" type="text" value={name} onChange={(e) => setName(e.currentTarget.value)} onBlur={(e) => updateName(e.currentTarget.value)} onKeyUp={(e) => onKeyLeave(e, updateName)} />
                    <label htmlFor="colour">Colour</label>
                    <Input id="colour" type="color" value={(tag.colour as string) ?? ""} onChange={(e) => save({ ...tag, colour: e.target.value })} />
                    <label htmlFor="exclude">Exclude from Reporting</label>
                    <Input.Switch id="exclude" checked={tag.settings?.excludeFromReporting} onChange={(e) => updateExcludeFromReporting(e.currentTarget.checked)} />
                    <label htmlFor="smooth">Allow Smoothing<Tooltip id="smoothing">Provides an option to average non-monthly transactions in trend reports</Tooltip></label>
                    <Input.Switch id="smooth" checked={tag.settings?.applySmoothing} onChange={(e) => updateAllowSmoothing(e.currentTarget.checked)} />
                    <label htmlFor="tags">Tags</label>
                    <TransactionTagTransactionTagPanel as="div" id="tags" tag={tag} alwaysShowEditPanel />
                </section>
            </Modal.Body>
            <Modal.Footer>
                <Button variant="primary" onClick={props.onHide}>Close</Button>
            </Modal.Footer>
        </Modal>
    );
}

export interface TransactionTagDetailsProps {
    tag: Tag
    show: boolean;
    onHide: () => void;
}
