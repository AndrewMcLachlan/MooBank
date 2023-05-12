import { Transaction, isCredit } from "models";
import { Button, Modal, } from "react-bootstrap";
import { TransactionTransactionTagPanel } from "./TransactionTransactionTagPanel";
import { useEffect, useState } from "react";
import { TransactionSearch } from "components";
import { TransactionDetailsIng } from "./TransactionDetailsIng";
import { formatCurrency } from "helpers";

export const TransactionDetails: React.FC<TransactionDetailsProps> = (props) => {

    const [transaction, setTransaction] = useState(props.transaction);
    const [notes, setNotes] = useState(props.transaction.notes ?? "");
    const [offsetBy, setOffsetBy] = useState<Transaction>(props.transaction.offsetBy);

    useEffect(() => setTransaction(props.transaction), [props.transaction]);

    if (!transaction) return null;

    return (
        <Modal show={props.show} onHide={props.onHide} size="lg">
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
                    {props.transaction.offsets &&
                        <>
                            <div>Rebate / refund for</div>
                            <div className="value"><span className="amount">{props.transaction.offsets?.amount}</span> - {props.transaction.offsets?.description}</div>
                        </>
                    }
                    <div>Tags</div>
                    <TransactionTransactionTagPanel as="div" transaction={props.transaction} alwaysShowEditPanel />
                </section>
                <section className="mt-3">
                    <div hidden={isCredit(props.transaction.transactionType)}>
                        <label>Corresponding rebate / refund</label>
                        <TransactionSearch value={offsetBy} onChange={(v) => setOffsetBy(v)} transaction={props.transaction} />
                    </div>
                    <div className="mt-3">
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