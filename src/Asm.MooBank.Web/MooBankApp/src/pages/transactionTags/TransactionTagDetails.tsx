import { Tooltip } from "components";
import { useUpdatingState } from "hooks";
import { TransactionTag } from "models";
import { Button, Form, Modal } from "react-bootstrap";
import { useUpdateTag } from "services";
import { TransactionTagTransactionTagPanel } from "./TransactionTagTransactionTagPanel";
import { onKeyLeave } from "helpers";

export const TransactionTagDetails: React.FC<TransactionTagDetailsProps> = (props) => {

    const [tag, setTag] = useUpdatingState(props.tag);
    const [name, setName] = useUpdatingState(props.tag.name);

    const updateTag = useUpdateTag();

    const updateExcludeFromReporting = (excludeFromReporting: boolean) => save({...tag, settings: {...tag.settings, excludeFromReporting}});
    const updateAllowSmoothing = (allowSmoothing: boolean) => save({...tag, settings: {...tag.settings, applySmoothing: allowSmoothing}});
    const updateName = (name: string) => save({...tag, name});

    const save = (newTag: TransactionTag) => {
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
                    <div>Name</div>
                    <Form.Control value={name} onChange={(e) => setName(e.currentTarget.value)} onBlur={(e) => updateName(e.currentTarget.value)} onKeyUp={(e) => onKeyLeave(e, updateName)} />
                    <div>Exclude from Reporting</div>
                    <Form.Check checked={tag.settings.excludeFromReporting} onChange={(e) => updateExcludeFromReporting(e.currentTarget.checked)} />
                    <div>Allow Smoothing<Tooltip id="smoothing">Provides an option to average non-monthly transactions in trend reports</Tooltip></div>
                    <Form.Check checked={tag.settings.applySmoothing} onChange={(e) => updateAllowSmoothing(e.currentTarget.checked)} />
                    <div>Tags</div>
                    <TransactionTagTransactionTagPanel as="div" tag={tag} alwaysShowEditPanel />
                </section>
            </Modal.Body>
            <Modal.Footer>
                <Button variant="primary" onClick={props.onHide}>Close</Button>
            </Modal.Footer>
        </Modal>
    );
}

export interface TransactionTagDetailsProps {
    tag: TransactionTag
    show: boolean;
    onHide: () => void;
}