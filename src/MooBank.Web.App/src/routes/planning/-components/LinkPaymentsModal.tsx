import { Button, Input, Modal, SpinnerContainer } from "@andrewmclachlan/moo-ds";
import { format, parseISO } from "date-fns";
import { useState } from "react";
import type { PlannedItem } from "api/types.gen";
import { Amount } from "components";
import { usePaymentCandidates, useSetPlannedItemPayments } from "../-hooks/usePlannedItemPayments";

interface LinkPaymentsModalProps {
    planId: string;
    item: PlannedItem;
    currencyCode: string;
    show: boolean;
    onHide: () => void;
}

/**
 * Lets the author say which payments are a planned item's.
 *
 * A tag is a category and a planned item is a specific project, so one "Home Improvements" tag
 * covers the solar panels, the fence and the renovation, and nothing but the author can say which
 * payment belongs to which. The tag narrows what is offered; the choosing happens here.
 */
export const LinkPaymentsModal: React.FC<LinkPaymentsModalProps> = ({ planId, item, currencyCode, show, onHide }) => {

    const { data: candidates, isLoading } = usePaymentCandidates(planId, item.id, show);
    const { setPayments, isPending } = useSetPlannedItemPayments();

    // Null until the author touches something, so the server's answer shows through without an
    // effect copying it into state and re-rendering.
    const [touched, setTouched] = useState<string[] | null>(null);

    const alreadyLinked = (candidates ?? []).filter(c => c.isLinked).map(c => c.transactionId);
    const selected = touched ?? alreadyLinked;

    const toggle = (transactionId: string) =>
        setTouched(selected.includes(transactionId)
            ? selected.filter(id => id !== transactionId)
            : [...selected, transactionId]);

    const selectedTotal = (candidates ?? [])
        .filter(c => selected.includes(c.transactionId))
        .reduce((sum, c) => sum + c.amount, 0);

    const handleSave = async () => {
        await setPayments(planId, item.id, selected);
        onHide();
    };

    return (
        <Modal show={show} onHide={onHide} size="lg" title={`Link Payments — ${item.name}`}>
            <Modal.Header closeButton>
                <Modal.Title>Link Payments — {item.name}</Modal.Title>
            </Modal.Header>
            <Modal.Body>
                {isLoading && <SpinnerContainer />}

                {!isLoading && !item.tagId && (
                    <p className="link-payments-empty">
                        Give this item a tag first. Payments are offered from the spending that carries it.
                    </p>
                )}

                {!isLoading && item.tagId && candidates?.length === 0 && (
                    <p className="link-payments-empty">
                        No payments carrying this item&rsquo;s tag within two months of its date.
                    </p>
                )}

                {!isLoading && !!candidates?.length && (
                    <>
                        <p className="link-payments-intro">
                            Payments tagged {item.tagName} within two months of {item.name}&rsquo;s date.
                            Anything already linked to another item is not shown.
                        </p>
                        <table className="link-payments">
                            <thead>
                                <tr>
                                    <th className="column-5"></th>
                                    <th className="column-15">Date</th>
                                    <th>Description</th>
                                    <th className="column-15">Amount</th>
                                </tr>
                            </thead>
                            <tbody>
                                {candidates.map(candidate => (
                                    <tr key={candidate.transactionId}>
                                        <td>
                                            <Input.Check
                                                type="checkbox"
                                                id={`payment-${candidate.transactionId}`}
                                                checked={selected.includes(candidate.transactionId)}
                                                onChange={() => toggle(candidate.transactionId)}
                                            />
                                        </td>
                                        <td>{format(parseISO(candidate.when), "dd MMM yyyy")}</td>
                                        <td className="link-payments-description">{candidate.description}</td>
                                        <td className="amount"><Amount amount={candidate.amount} currencyCode={currencyCode} /></td>
                                    </tr>
                                ))}
                            </tbody>
                            <tfoot>
                                <tr>
                                    <td colSpan={3}>{selected.length} selected, against a plan of</td>
                                    <td className="amount"><Amount amount={item.amount} currencyCode={currencyCode} /></td>
                                </tr>
                                <tr>
                                    <td colSpan={3}>Linked total</td>
                                    <td className="amount"><Amount amount={selectedTotal} currencyCode={currencyCode} /></td>
                                </tr>
                            </tfoot>
                        </table>
                    </>
                )}
            </Modal.Body>
            <Modal.Footer>
                <Button variant="outline-primary" onClick={onHide}>Close</Button>
                <Button variant="primary" onClick={handleSave} disabled={isPending || isLoading || !item.tagId}>Save</Button>
            </Modal.Footer>
        </Modal>
    );
};
