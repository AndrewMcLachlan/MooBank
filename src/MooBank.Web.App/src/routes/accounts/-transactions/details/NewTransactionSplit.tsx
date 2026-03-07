import React, { useState } from "react";

import { SaveIcon } from "@andrewmclachlan/moo-ds";
import { valueAsNumber } from "utils/valueAsNumber";
import type { Transaction, TransactionSplit } from "api/types.gen";
import { emptyTransactionSplit } from "models/transactions";
import { Col, Input, Row } from "@andrewmclachlan/moo-ds";
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
        <Row>
            <Col sm={9}>
                <TransactionSplitTagPanel as="div" transactionId={transaction.id} onChange={(s) => splitChanged({ ...split, tags: s.tags })} alwaysShowEditPanel transactionSplit={split} />
            </Col>
            <Col sm={3} className="split-controls">
                <Input type="number" value={split.amount} required min={0} max={transaction.amount} onChange={(s) => splitChanged({ ...split, amount: valueAsNumber(s.currentTarget) })} />
                {/*<Input.Feedback type="invalid">Please enter an amount</Input.Feedback>*/}
                <SaveIcon onClick={saveClick} />
            </Col>
        </Row>
    );
}

export interface NewTransactionSplitProps {
    transaction: Transaction;
    onSave: (split: TransactionSplit) => void;
}
