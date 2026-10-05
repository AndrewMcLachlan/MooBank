import React from "react";
import { Button, Skeleton, formatCurrency } from "@andrewmclachlan/moo-ds";
import { ResponsiveDialog } from "components/ResponsiveDialog";

import type { Transaction } from "api/types.gen";
import { useTransactionDeleteImpact } from "routes/accounts/-hooks/useTransactionDeleteImpact";

export const DeleteTransaction: React.FC<DeleteTransactionProps> = ({ transaction, show, onCancel, onConfirm, isDeleting }) => {

    const impact = useTransactionDeleteImpact(transaction.accountId, transaction.id, show);

    const refunds = impact.data?.refunds ?? [];
    const plannedItems = impact.data?.plannedItems ?? [];

    return (
        <ResponsiveDialog show={show} onHide={onCancel} size="lg" className="delete-transaction">
            <ResponsiveDialog.Header closeButton>
                <ResponsiveDialog.Title>Delete Transaction</ResponsiveDialog.Title>
            </ResponsiveDialog.Header>
            <ResponsiveDialog.Body>
                <p>
                    Delete <strong>{transaction.description}</strong> for {formatCurrency(transaction.amount)}? This cannot be undone.
                </p>
                {impact.isLoading && <Skeleton.Text lines={2} />}
                {!impact.isLoading && refunds.length > 0 &&
                    <section className="delete-impact">
                        <h4>This transaction is a refund</h4>
                        <p>Deleting it puts the full amount back on:</p>
                        <ul>
                            {refunds.map(refund =>
                                <li key={`${refund.description}-${refund.amount}`}>{refund.description} ({formatCurrency(refund.amount)})</li>
                            )}
                        </ul>
                    </section>
                }
                {!impact.isLoading && plannedItems.length > 0 &&
                    <section className="delete-impact">
                        <h4>This transaction is a planned payment</h4>
                        <p>It will be unlinked from:</p>
                        <ul>
                            {plannedItems.map(item =>
                                <li key={`${item.planName}-${item.itemName}`}>{item.itemName} ({item.planName})</li>
                            )}
                        </ul>
                    </section>
                }
            </ResponsiveDialog.Body>
            <ResponsiveDialog.Footer>
                <Button variant="outline-primary" onClick={onCancel}>Cancel</Button>
                <Button variant="danger" disabled={isDeleting} onClick={onConfirm}>Delete</Button>
            </ResponsiveDialog.Footer>
        </ResponsiveDialog>
    );
};

export interface DeleteTransactionProps {
    transaction: Transaction;
    show: boolean;
    onCancel: () => void;
    onConfirm: () => void;
    isDeleting?: boolean;
}

DeleteTransaction.displayName = "DeleteTransaction";
