import { Transaction, TransactionOffset, TransactionSplit, isCredit, getSplitTotal, isDebit } from "models";
import { Button, Col, Form, Modal, Row, } from "react-bootstrap";
import { TransactionSplit as TransactionSplitPanel } from "./TransactionSplit";
import React, { useEffect, useMemo, useState } from "react";
import { ExtraInfo } from "./ExtraInfo";
import { formatCurrency } from "@andrewmclachlan/mooapp";
import { NewTransactionSplit } from "./NewTransactionSplit";

export const TransactionDetails: React.FC<TransactionDetailsProps> = (props) => {

    const transaction = useMemo(() => props.transaction, [props.transaction]);
    const [notes, setNotes] = useState(props.transaction.notes ?? "");
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

    if (!transaction) return null;

    return (
        <Modal show={props.show} onHide={props.onHide} size="xl" className="transaction-details">
            <Modal.Header closeButton>
                <Modal.Title>Transaction</Modal.Title>
            </Modal.Header>
            <Modal.Body>
                <section className="transaction-info">
                    <section>
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
                        <div>Who</div>
                        <div className="value">{props.transaction.accountHolderName}</div>
                        <div>Location</div>
                        <div className="value">{props.transaction.location}</div>
                    </section>
                    {props.transaction.extraInfo && <ExtraInfo transaction={transaction} />}
                </section>
                <section className="offset-for" hidden={props.transaction.offsetFor?.length === 0}>
                    {props.transaction.offsetFor?.map((to) =>
                        <React.Fragment key={to.transaction.id}>
                            <div>Rebate / refund for</div>
                            <div className="value"><span className="amount">{to.amount}</span> - {to.transaction.description}</div>
                        </React.Fragment>
                    )}
                </section>
                <section className="notes">
                    <label>Notes</label>
                    <textarea className="form-control" value={notes} onChange={(e) => setNotes(e.currentTarget.value)} />
                </section>
                <section className="splits">
                    <h4>Tags{isDebit(props.transaction.transactionType) && <> &amp; Refunds</>}</h4>
                    <Row>
                        <Col xl={9}>
                            <Form.Label>Tags</Form.Label>
                        </Col>
                        <Col xl={2}>
                            <Form.Label>Amount</Form.Label>
                        </Col>
                    </Row>
                    <div>
                        {splits?.map((split) =>
                            <React.Fragment key={split.id}>
                                <TransactionSplitPanel key={split.id} transaction={transaction} split={split} splitChanged={splitChanged} removeSplit={removeSplit} />
                            </React.Fragment>
                        )}
                    </div>
                    <div>
                        <Row>
                            <Col xl={9}>
                                <Form.Label>Split Transaction</Form.Label>
                            </Col>
                        </Row>
                        <NewTransactionSplit transaction={transaction} splitTotal={getSplitTotal(splits)} onSave={addSplit} />
                    </div>
                </section>
            </Modal.Body>
            <Modal.Footer>
                <Button variant="secondary" onClick={props.onHide}>Close</Button>
                <Button variant="primary" onClick={() => { props.onSave(notes, splits) }}>Save</Button>
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
