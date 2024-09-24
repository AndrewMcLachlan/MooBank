import React, { useState } from "react";

import { SaveIcon } from "@andrewmclachlan/mooapp";
import { valueAsNumber } from "helpers";
import { Transaction, TransactionSplit, emptyTransactionSplit } from "models";
import { Col, Form, Row } from "react-bootstrap";
import { TransactionSplitTagPanel } from "./TransactionSplitTagPanel";


export const NewTransactionSplit: React.FC<NewTransactionSplitProps> = ({ transaction,  onSave }) => {

    const [split, setSplit] = useState(emptyTransactionSplit);

    const splitChanged = (split: TransactionSplit) => {
        setSplit(split);
    };

    const saveClick = () => {
        onSave(split);
        setSplit(emptyTransactionSplit);
    }

    return (
        <Form.Group as={Row}>
            <Col sm={9}>
                <TransactionSplitTagPanel as="div" transactionId={transaction.id} onChange={(s) => splitChanged({ ...split, tags: s.tags })} alwaysShowEditPanel transactionSplit={split} />
            </Col>
            <Col sm={2}>
                <Form.Control type="number" value={split.amount} required min={0} max={transaction.amount} onChange={(s) => splitChanged({ ...split, amount: valueAsNumber(s.currentTarget) })} />
                <Form.Control.Feedback type="invalid">Please enter an amount</Form.Control.Feedback>
            </Col>
            <Col className="delete-offset">
                <SaveIcon onClick={saveClick} />
            </Col>
        </Form.Group>
    );
}

export interface NewTransactionSplitProps {
    transaction: Transaction;
    onSave: (split: TransactionSplit) => void;
}
