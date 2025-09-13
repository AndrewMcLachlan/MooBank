import React from "react";
import { Tooltip, useUpdatingState } from "@andrewmclachlan/moo-ds";
import { Tag } from "models";
import { Button, Form, Modal } from "react-bootstrap";
import { useUpdateTag } from "services";
import { TransactionTagTransactionTagPanel } from "./TagTagPanel";
import { onKeyLeave } from "helpers";

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
                    <Form.Control id="name" placeholder="Name" type="text" value={name} onChange={(e) => setName(e.currentTarget.value)} onBlur={(e) => updateName(e.currentTarget.value)} onKeyUp={(e) => onKeyLeave(e, updateName)} />
                    <label htmlFor="colour">Colour</label>
                    <Form.Control id="colour" type="color" value={tag.colour ?? ""} onChange={(e) => save({ ...tag, colour: e.target.value })} />
                    <label htmlFor="exclude">Exclude from Reporting</label>
                    <Form.Switch id="exclude" checked={tag.settings?.excludeFromReporting} onChange={(e) => updateExcludeFromReporting(e.currentTarget.checked)} />
                    <label htmlFor="smooth">Allow Smoothing<Tooltip id="smoothing">Provides an option to average non-monthly transactions in trend reports</Tooltip></label>
                    <Form.Switch id="smooth" checked={tag.settings?.applySmoothing} onChange={(e) => updateAllowSmoothing(e.currentTarget.checked)} />
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
