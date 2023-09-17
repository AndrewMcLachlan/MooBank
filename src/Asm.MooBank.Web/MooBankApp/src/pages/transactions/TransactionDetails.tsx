import { Transaction, TransactionOffset, TransactionSplit, isCredit, getSplitTotal } from "models";
import { Button, Col, Form, Modal, Row, } from "react-bootstrap";
import { TransactionSplitPanel } from "./TransactionSplitPanel";
import { useEffect, useMemo, useState } from "react";
import { TransactionSearch } from "components";
import { TransactionDetailsIng } from "./TransactionDetailsIng";
import { ClickableIcon, IconButton, Input, formatCurrency } from "@andrewmclachlan/mooapp";
import { useInvalidateSearch } from "services";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { NewTransactionSplit } from "./NewTransactionSplit";
import { valueAsNumber } from "helpers";

export const TransactionDetails: React.FC<TransactionDetailsProps> = (props) => {

    const invalidateSearch = useInvalidateSearch(props.transaction.id);

    const transaction = useMemo(() => props.transaction, [props.transaction]);
    const [notes, setNotes] = useState(props.transaction.notes ?? "");
    const [offsetBy, setOffsetBy] = useState<TransactionOffset[]>(props.transaction.offsetBy);
    const [splits, setSplits] = useState<TransactionSplit[]>(props.transaction.splits ?? []);

    const addSplit = (split: TransactionSplit) => {
        setSplits([...splits, split]);
    }

    const splitChanged = (split: TransactionSplit) => {

        const splitTotal = getSplitTotal(splits) - split.amount;
        const maxSplit = Math.abs(transaction.amount) - splitTotal;

        if (split.amount > maxSplit || split.amount < 0) split.amount = maxSplit;

        const newSplits = [...splits];
        newSplits.splice(newSplits.findIndex(o => o.id === split.id), 1, split);
        setSplits(newSplits);
    };

    const removeSplit = (id: string) => {
        const newSplits = splits.filter(s => s.id !== id);
        setSplits(newSplits);
    }

    const offsetChanged = (offset: TransactionOffset) => {
        if (offset.amount > offset.transaction.amount || offset.amount <= 0) offset.amount = offset.transaction.amount;

        const newOffsetBy = [...offsetBy];
        const index = newOffsetBy.findIndex(o => o.transaction.id === offset.transaction.id);
        if (index === -1) {
            newOffsetBy.push(offset);
        } else {
            newOffsetBy[index] = offset;
        }
        invalidateSearch();
        setOffsetBy(newOffsetBy);
    };

    const removeOffset = (transactionId: string) => {
        const newOffsetBy = offsetBy.filter(o => o.transaction.id !== transactionId);
        invalidateSearch();
        setOffsetBy(newOffsetBy);
    }

    if (!transaction) return null;

    return (
        <Modal show={props.show} onHide={props.onHide} size="xl">
            <Modal.Header closeButton>
                <Modal.Title>Transaction</Modal.Title>
            </Modal.Header>
            <Modal.Body>
                <section className="transaction-details">
                    <div>Amount</div>
                    <div className="value amount">{formatCurrency(props.transaction.amount)}</div>
                    {props.transaction.netAmount !== props.transaction.amount &&
                        <>
                            <div>Net Amount</div>
                            <div className="value amount">{formatCurrency(props.transaction.netAmount)}</div>
                        </>
                    }
                    <div>Description</div>
                    <div className="value description">{props.transaction.description}</div>
                    {props.transaction.extraInfo && <TransactionDetailsIng transaction={transaction} />}
                    {props.transaction.offsets?.map((to) =>
                        <>
                            <div>Rebate / refund for</div>
                            <div className="value"><span className="amount">{to.amount}</span> - {to.transaction.description}</div>
                        </>
                    )}
                </section>
                <section className="splits">
                    <Row>
                        <Col xl={9}>
                            <Form.Label>Tags</Form.Label>
                        </Col>
                        <Col xl={2}>
                            <Form.Label>Amount</Form.Label>
                        </Col>
                    </Row>
                    {splits?.map((split) =>
                        <Form.Group as={Row} key={JSON.stringify(split)}>
                            <Col xl={9}>
                                <TransactionSplitPanel as="div" transactionSplit={split} transactionId={props.transaction.id} alwaysShowEditPanel onChange={(s) => splitChanged({ ...split, tags: s.tags })} />
                            </Col>
                            <Col xl={2}>
                                <Form.Control type="number" value={split.amount} required max={props.transaction.amount} onChange={(e) => splitChanged({ ...split, amount: valueAsNumber(e.currentTarget) })} />
                                <Form.Control.Feedback type="invalid">Please enter an amount</Form.Control.Feedback>
                            </Col>
                            <Col className="delete-offset">
                                <ClickableIcon icon="trash-alt" onClick={() => removeSplit(split.id)} />
                            </Col>
                        </Form.Group>
                    )}
                    <NewTransactionSplit transaction={transaction} splitTotal={getSplitTotal(splits)} onSave={addSplit} />
                </section>
                <section className="offsets" hidden={isCredit(props.transaction.transactionType)}>
                    <Row>
                        <Col xl={9}>
                            <Form.Label>Corresponding rebate / refund</Form.Label>
                        </Col>
                        <Col xl={2}>
                            <Form.Label>Amount</Form.Label>
                        </Col>
                    </Row>
                    {offsetBy?.map((to) =>
                        <Form.Group as={Row} key={to.transaction.id}>
                            <Col xl={9}>
                                <TransactionSearch value={to.transaction} onChange={(v) => offsetChanged({ ...to, transaction: v })} transaction={props.transaction} />
                            </Col>
                            <Col xl={2}>
                                <Form.Control type="number" value={to.amount} required max={to.transaction.amount} onChange={e => offsetChanged({ ...to, amount: (e.currentTarget as any).valueAsNumber })} />
                                <Form.Control.Feedback type="invalid">Please enter an amount</Form.Control.Feedback>
                            </Col>
                            <Col className="delete-offset">
                                <FontAwesomeIcon icon="trash-alt" onClick={() => removeOffset(to.transaction.id)} />
                            </Col>
                        </Form.Group>
                    )}
                    <Form.Group as={Row}>
                        <Col xl={9}>
                            <TransactionSearch onChange={(v) => offsetChanged({ transaction: v, amount: v.amount })} transaction={props.transaction} excludedTransactions={offsetBy?.map(o => o.transaction.id)} />
                        </Col>
                    </Form.Group>
                </section>
                <section className="mt-3">
                    <label>Notes</label>
                    <textarea className="form-control" value={notes} onChange={(e) => setNotes(e.currentTarget.value)} />
                </section>

            </Modal.Body>
            <Modal.Footer>
                <Button variant="secondary" onClick={props.onHide}>Close</Button>
                <Button variant="primary" onClick={() => { props.onSave(notes, splits, offsetBy)}}>Save</Button>
            </Modal.Footer>
        </Modal >
    );
}

export interface TransactionDetailsProps {
    transaction: Transaction;
    show: boolean;
    onHide: () => void;
    onSave: (notes?: string, splits?: TransactionSplit[], offsetBy?: TransactionOffset[]) => void;
}