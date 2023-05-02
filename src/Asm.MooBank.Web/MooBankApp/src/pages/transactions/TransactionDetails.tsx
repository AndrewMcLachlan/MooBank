import "./TransactionDetails.scss";

import { Transaction } from "models";
import { Button, Modal, } from "react-bootstrap";
import { TransactionTransactionTagPanel } from "./TransactionTransactionTagPanel";
import { useState } from "react";
import { TransactionSearch } from "components";

export const TransactionDetails: React.FC<TransactionDetailsProps> = (props) => {

    const [notes, setNotes] = useState(props.transaction.notes ?? "");
    const [offsetBy, setOffsetBy] = useState<Transaction>(props.transaction.offsetBy);

    return (
        <Modal show={props.show} onHide={props.onHide} size="lg">
            <Modal.Header closeButton>
                <Modal.Title>Transaction</Modal.Title>
            </Modal.Header>
            <Modal.Body>
                <section className="transaction-details">
                    <div>Amount</div>
                    <div className="value">{props.transaction.amount}</div>
                    <div>Description</div>
                    <div className="value description">{props.transaction.description}</div>
                    <div>Tags</div>
                    <TransactionTransactionTagPanel as="div" transaction={props.transaction} />
                </section>
                <section className="mt-3">
                <div>
                    <label>Corresponding rebate/refund</label>
                    <TransactionSearch value={offsetBy} onChange={(v) => setOffsetBy(v)} transaction={props.transaction} />
                </div>
                <div  className="mt-3">
                    <label>Notes</label>
                    <textarea className="form-control" value={notes} onChange={(e) => setNotes(e.currentTarget.value)} />
                </div>
                </section>
            </Modal.Body>
            <Modal.Footer>
                <Button variant="secondary" onClick={props.onHide}>Close</Button>
                <Button variant="primary" onClick={() => props.onSave(notes, offsetBy)}>Save</Button>
            </Modal.Footer>
        </Modal>
    );
}

export interface TransactionDetailsProps {
    transaction: Transaction;
    show: boolean;
    onHide: () => void;
    onSave: (notes?: string, offsetBy?: Transaction) => void;
}