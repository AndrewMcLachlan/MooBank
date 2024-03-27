import { formatCurrency } from "@andrewmclachlan/mooapp";
import { format } from "date-fns/format";
import { parseISO } from "date-fns/parseISO";
import { Transaction, TransactionOffset, TransactionSplit, getSplitTotal, isDebit } from "models";
import React, { useEffect, useMemo, useState } from "react";
import { Button, Modal, OverlayTrigger, Popover } from "react-bootstrap";

import { ExtraInfo } from "../ExtraInfo";
import { TransactionSplits } from "./TransactionSplits";

export const TransactionDetails: React.FC<TransactionDetailsProps> = (props) => {

    const transaction = useMemo(() => props.transaction, [props.transaction]);
    const [notes, setNotes] = useState(props.transaction.notes ?? "");
    const [excludeFromReporting, setExcludeFromReporting] = useState(props.transaction.excludeFromReporting ?? false);
    const [splits, setSplits] = useState<TransactionSplit[]>(transaction.splits ?? []);

    useEffect(() => {
        setNotes(props.transaction.notes ?? "");
        setExcludeFromReporting(props.transaction.excludeFromReporting ?? false);
    }, [transaction]);

    if (!transaction) return null;

    const invalidSplits = getSplitTotal(splits) !== Math.abs(transaction.amount);

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
                        <div>Purchase Date</div>
                        <div className="value">{props.transaction.purchaseDate ? format(parseISO(props.transaction.purchaseDate), "dd/MM/yyyy") : "-"}</div>
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
                <section>
                    <label className="form-label">Exclude from reporting</label>
                    <div>
                        <input type="checkbox" className="form-check-input" checked={excludeFromReporting} onChange={(e) => setExcludeFromReporting(e.currentTarget.checked)} />
                    </div>
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
                <Button variant="secondary" onClick={props.onHide}>Close</Button>
                {invalidSplits &&
                <OverlayTrigger placement="top" overlay={<Popover><Popover.Body>The total of the splits must equal the transaction amount</Popover.Body></Popover>} >
                    <div><Button variant="primary" disabled>Save</Button></div>
                </OverlayTrigger>
                }
                {!invalidSplits && <Button variant="primary" onClick={() => { props.onSave(excludeFromReporting, notes, splits) }}>Save</Button>}
            </Modal.Footer>
        </Modal >
    );
}

export interface TransactionDetailsProps {
    transaction: Transaction;
    show: boolean;
    onHide: () => void;
    onSave: (excludeFromReporting: boolean, notes?: string, splits?: TransactionSplit[], offsetBy?: TransactionOffset[]) => void;
}
