import { formatCurrency } from "@andrewmclachlan/moo-ds";
import { format } from "date-fns/format";
import { parseISO } from "date-fns/parseISO";
import type { Transaction, TransactionSplit } from "api/types.gen";
import { getSplitTotal, isDebit } from "helpers/transactions";
import React, { useEffect, useMemo, useState } from "react";
import { Button, Col, Modal, OverlayTrigger, Popover, Row } from "@andrewmclachlan/moo-ds";

import { ExtraInfo } from "./ExtraInfo";
import { TransactionSplits } from "./TransactionSplits";
import { notEquals } from "helpers/equals";
import { useUpdateTransaction } from "services";
import { Amount } from "components/Amount";

export const TransactionDetails: React.FC<TransactionDetailsProps> = (props) => {

    // This looks odd.
    const transaction = useMemo(() => props.transaction, [props.transaction]);
    const [notes, setNotes] = useState(props.transaction?.notes ?? "");
    const [excludeFromReporting, setExcludeFromReporting] = useState(props.transaction?.excludeFromReporting ?? false);
    const [splits, setSplits] = useState<TransactionSplit[]>(transaction?.splits ?? []);

    useEffect(() => {
        setNotes(props.transaction?.notes ?? "");
        setExcludeFromReporting(props.transaction?.excludeFromReporting ?? false);
    }, [transaction]);

    const updateTransaction = useUpdateTransaction();

    const onSave = (excludeFromReporting: boolean, notes: string, splits: TransactionSplit[]) => {
        updateTransaction.mutateAsync(transaction.accountId, transaction.id, { excludeFromReporting, notes, splits });
        props.onSave();
    }

    if (!transaction) return null;

    const invalidSplits = notEquals(getSplitTotal(splits), Math.abs(transaction.amount));

    return (
        <Modal show={props.show} onHide={props.onHide} size="xl" className="transaction-details">
            <Modal.Header closeButton>
                <Modal.Title>Transaction</Modal.Title>
            </Modal.Header>
            <Modal.Body>
                <section className="transaction-info row">
                    <section>
                        <div>Amount</div>
                        <div className="value amount"><Amount amount={props.transaction.amount} minus /></div>
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
                        <div>Purchase Date</div>
                        <div className="value">{props.transaction.purchaseDate ? format(parseISO(props.transaction.purchaseDate), "dd/MM/yyyy") : "-"}</div>
                    </section>
                    {props.transaction.extraInfo && <ExtraInfo transaction={transaction} />}
                </section>
                <section className="offset-for" hidden={props.transaction.offsetFor?.length === 0}>
                    <Row>
                        {props.transaction.offsetFor?.map((to) =>
                            <Col xl={6} key={to.transaction.id}>
                                <div>Rebate / refund for</div>
                                <div className="value"><span className="amount">{to.amount}</span> - {to.transaction.description}</div>
                            </Col>
                        )}
                    </Row>
                </section>
                <section className="form-check">
                    <input id="exclude" type="checkbox" className="form-check-input" checked={excludeFromReporting} onChange={(e) => setExcludeFromReporting(e.currentTarget.checked)} />
                    <label className="form-check-label" htmlFor="exclude">Exclude from reporting</label>
                </section>
                <section className="notes">
                    <label>Notes</label>
                    <textarea className="form-control" value={notes} onChange={(e) => setNotes(e.currentTarget.value)} />
                </section>
                <section className="splits">
                    <h4>Tags{isDebit(props.transaction.transactionType) && <> &amp; Refunds</>}</h4>
                    <TransactionSplits transaction={transaction} onChange={setSplits} />
                </section>
            </Modal.Body>
            <Modal.Footer>
                <Button variant="outline-primary" onClick={props.onHide}>Close</Button>
                {invalidSplits &&
                    <OverlayTrigger placement="top" overlay={<Popover id="splits-popover"><Popover.Body>The total of the splits must equal the transaction amount</Popover.Body></Popover>} >
                        <div><Button variant="primary" disabled>Save</Button></div>
                    </OverlayTrigger>
                }
                {!invalidSplits && <Button variant="primary" disabled={updateTransaction.isPending} onClick={() => { onSave(excludeFromReporting, notes, splits) }}>Save</Button>}
            </Modal.Footer>
        </Modal >
    );
}

export interface TransactionDetailsProps {
    transaction: Transaction;
    show: boolean;
    onHide: () => void;
    onSave: () => void;
}
