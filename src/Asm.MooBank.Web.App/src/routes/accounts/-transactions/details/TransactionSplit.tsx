import { TransactionSearch } from "components";
import { DeleteIcon } from "@andrewmclachlan/moo-ds";
import { valueAsNumber } from "helpers";
import type { Transaction, TransactionSplit as TransactionSplitModel, TransactionOffsetFor } from "api/types.gen";
import { isCredit } from "helpers/transactions";
import React, { useState } from "react";
import { Col, Form, Input, Row } from "@andrewmclachlan/moo-ds";
import { useInvalidateSearch } from "services";
import { TransactionSplitTagPanel } from "./TransactionSplitTagPanel";

export const TransactionSplit: React.FC<TransactionSplitProps> = ({ transaction, split, splitChanged, removeSplit }) => {

    const [offsetBy, setOffsetBy] = useState<TransactionOffsetFor[]>(split.offsetBy);
    const invalidateSearch = useInvalidateSearch(transaction.id);

    const offsetChanged = (offset: TransactionOffsetFor, oldOffset?: Transaction) => {
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
            <Row>
                <Col sm={9}>
                    <Form.Label>Tags</Form.Label>
                    <TransactionSplitTagPanel as="div" transactionSplit={split} transactionId={transaction.id} alwaysShowEditPanel onChange={(s) => splitChanged({ ...split, tags: s.tags })} />
                </Col>
                <Col sm={3}>
                    <Form.Label>Amount</Form.Label>
                    <div className="split-controls">
                        <Input type="number" value={split.amount} required max={transaction.amount} onChange={(e) => splitChanged({ ...split, amount: valueAsNumber(e.currentTarget) })} />
                        {/*<Input.Feedback type="invalid">Please enter an amount</Input.Feedback>*/}
                        <DeleteIcon onClick={() => removeSplit(split.id)} />
                    </div>
                </Col>
            </Row>
            <section className="offsets" hidden={isCredit(transaction.transactionType)}>
                <Form.Label>Corresponding rebate / refund</Form.Label>

                {offsetBy?.map((to, index) =>
                    <Row key={to.transaction.id}>
                        <Col sm={9}>
                            <TransactionSearch value={to.transaction} onChange={(v) => offsetChanged({ ...to, transaction: v }, to.transaction)} transaction={transaction} excludedTransactions={offsetBy.map(ob => ob.transaction.id)} />
                        </Col>
                        <Col sm={3} className="offset-controls">
                            <Input type="number" value={to.amount} required max={to.transaction.amount} onChange={e => offsetChanged({ ...to, amount: valueAsNumber(e.currentTarget) })} />
                            {/* <Input.Feedback type="invalid">Please enter an amount</Input.Feedback> */}
                            <DeleteIcon onClick={() => removeOffset(to.transaction.id)} />
                        </Col>
                    </Row>
                )}
                <Row>
                    <Col sm={9}>
                        <TransactionSearch onChange={(v) => offsetChanged({ transaction: v, amount: v.amount })} transaction={transaction} excludedTransactions={offsetBy?.map(o => o.transaction.id)} />
                    </Col>
                </Row>
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
