import { TransactionSearch } from "components";
import { DeleteIcon } from "@andrewmclachlan/mooapp";
import { valueAsNumber } from "helpers";
import { Transaction, TransactionOffset, TransactionSplit as TransactionSplitModel, isCredit } from "models";
import React, { useState } from "react";
import { Col, Form, Row } from "react-bootstrap";
import { useInvalidateSearch } from "services";
import { TransactionSplitTagPanel } from "./TransactionSplitTagPanel";

export const TransactionSplit: React.FC<TransactionSplitProps> = ({ transaction, split, splitChanged, removeSplit }) => {

    const [offsetBy, setOffsetBy] = useState<TransactionOffset[]>(split.offsetBy);
    const invalidateSearch = useInvalidateSearch(transaction.id);

    const offsetChanged = (offset: TransactionOffset, oldOffset?: Transaction) => {
        if (offset.amount > offset.transaction.amount || offset.amount <= 0) offset.amount = offset.transaction.amount;

        const newOffsetBy = [...offsetBy];
        let index = newOffsetBy.findIndex(o => o.transaction.id === oldOffset?.id)
        index = index == -1 ? newOffsetBy.findIndex(o => o.transaction.id === offset.transaction.id) : index;
        if (index === -1) {
            newOffsetBy.push(offset);
        } else {
            newOffsetBy[index] = offset;
        }
        invalidateSearch();
        setOffsetBy(newOffsetBy);
        splitChanged({ ...split, offsetBy: newOffsetBy });
    };

    const removeOffset = (transactionId: string) => {
        const newOffsetBy = offsetBy.filter(o => o.transaction.id !== transactionId);
        invalidateSearch();
        setOffsetBy(newOffsetBy);
        splitChanged({ ...split, offsetBy: newOffsetBy });
    }

    return (
        <>
            <Form.Group as={Row}>
                <Col sm={9}>
                    <Form.Label>Tags</Form.Label>
                    <TransactionSplitTagPanel as="div" transactionSplit={split} transactionId={transaction.id} alwaysShowEditPanel onChange={(s) => splitChanged({ ...split, tags: s.tags })} />
                </Col>
                <Col sm={3}>
                    <Form.Label>Amount</Form.Label>
                    <div className="split-controls">
                        <Form.Control type="number" value={split.amount} required max={transaction.amount} onChange={(e) => splitChanged({ ...split, amount: valueAsNumber(e.currentTarget) })} />
                        <Form.Control.Feedback type="invalid">Please enter an amount</Form.Control.Feedback>
                        <DeleteIcon onClick={() => removeSplit(split.id)} />
                    </div>
                </Col>
            </Form.Group>
            <section className="offsets" hidden={isCredit(transaction.transactionType)}>
                <Form.Label>Corresponding rebate / refund</Form.Label>

                {offsetBy?.map((to, index) =>
                    <Form.Group as={Row} key={to.transaction.id}>
                        <Col sm={9}>
                            <TransactionSearch value={to.transaction} onChange={(v) => offsetChanged({ ...to, transaction: v }, to.transaction)} transaction={transaction} excludedTransactions={offsetBy.map(ob => ob.transaction.id)} />
                        </Col>
                        <Col sm={3} className="offset-controls">
                            <Form.Control type="number" value={to.amount} required max={to.transaction.amount} onChange={e => offsetChanged({ ...to, amount: valueAsNumber(e.currentTarget) })} />
                            <Form.Control.Feedback type="invalid">Please enter an amount</Form.Control.Feedback>
                            <DeleteIcon onClick={() => removeOffset(to.transaction.id)} />
                        </Col>
                    </Form.Group>
                )}
                <Form.Group as={Row}>
                    <Col sm={9}>
                        <TransactionSearch onChange={(v) => offsetChanged({ transaction: v, amount: v.amount })} transaction={transaction} excludedTransactions={offsetBy?.map(o => o.transaction.id)} />
                    </Col>
                </Form.Group>
            </section>
        </>
    );
}

export interface TransactionSplitProps {
    transaction: Transaction;
    split: TransactionSplitModel;
    splitChanged: (split: TransactionSplitModel) => void;
    removeSplit: (splitId: string) => void;
}
