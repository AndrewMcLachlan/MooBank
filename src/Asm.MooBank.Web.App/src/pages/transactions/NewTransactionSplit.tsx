import { useState } from "react";

import { Col, Form, Row } from "react-bootstrap";
import { TransactionSplitTagPanel } from "./TransactionSplitTagPanel";
import { ClickableIcon } from "@andrewmclachlan/mooapp";
import { Transaction, TransactionSplit, emptyTransactionSplit } from "models";
import { valueAsNumber } from "helpers";


export const NewTransactionSplit: React.FC<NewTransactionSplitProps> = ({ transaction, splitTotal, onSave }) => {

    const [split, setSplit] = useState(emptyTransactionSplit);

    const maxSplit = Math.abs(transaction.amount) - splitTotal;

    const splitChanged = (split: TransactionSplit) => {

        if (split.amount > maxSplit || split.amount < 0) split.amount = maxSplit;

        setSplit(split);
    };

    const saveClick = () => {
        onSave(split);
        setSplit(emptyTransactionSplit);
    }

    return (
        <Form.Group as={Row}>
            <Col xl={9}>
                <TransactionSplitTagPanel as="div" transactionId={transaction.id} onChange={(s) => splitChanged({ ...split, tags: s.tags })} alwaysShowEditPanel transactionSplit={split} />
            </Col>
            <Col xl={2}>
                <Form.Control type="number" value={split.amount} required min={0} max={maxSplit} onChange={(s) => splitChanged({ ...split, amount: valueAsNumber(s.currentTarget) })} />
                <Form.Control.Feedback type="invalid">Please enter an amount</Form.Control.Feedback>
            </Col>
            <Col className="delete-offset">
                <ClickableIcon icon="check-circle" onClick={saveClick} />
            </Col>
        </Form.Group>
    );
}

export interface NewTransactionSplitProps {
    transaction: Transaction;
    splitTotal: number;
    onSave: (split: TransactionSplit) => void;
}