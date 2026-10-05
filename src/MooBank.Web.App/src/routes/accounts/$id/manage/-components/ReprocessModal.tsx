import React, { useState } from "react";
import { Button } from "@andrewmclachlan/moo-ds";
import { useAccount } from "components";
import { ResponsiveDialog } from "components/ResponsiveDialog";
import type { LogicalAccount } from "api/types.gen";
import { useReprocessTransactions } from "routes/accounts/-hooks/useReprocessTransactions";

export const ReprocessModal: React.FC<ReprocessModalProps> = ({ instrumentId, onClose }) => {

    const account = useAccount() as LogicalAccount;

    const reprocessTransactions = useReprocessTransactions();

    const openAccounts = account.institutionAccounts.filter(ia => ia.closedDate === null);

    const [institutionAccountId, setInstitutionAccountId] = useState<string | null>(openAccounts[0]?.id ?? null);

    if (account?.controller !== "Import") {
        return null;
    }

    const submitClick = () => {
        if (!institutionAccountId) return;
        reprocessTransactions(instrumentId, institutionAccountId);
        onClose();
    }

    return (
        <ResponsiveDialog className="import" show onHide={onClose} size="lg">
            <ResponsiveDialog.Header closeButton>
                <ResponsiveDialog.Title>Reprocess Imported Transactions</ResponsiveDialog.Title>
            </ResponsiveDialog.Header>
            <ResponsiveDialog.Body>
                <div>
                    <div className="import-types" hidden={openAccounts.length <= 1}>
                        {openAccounts.map(ia => {
                            return (
                                <div key={ia.id} className={`import-type-option ${institutionAccountId === ia.id ? 'selected' : ''}`} onClick={() => setInstitutionAccountId(ia.id)}>
                                    <input type="radio" id={ia.id} name="institutionAccountId" value={ia.id} className="form-check-input" checked={institutionAccountId === ia.id} onChange={() => { setInstitutionAccountId(ia.id); }} />
                                    <label htmlFor={ia.id} className="form-label">{ia.name ?? ia.institutionId}</label>
                                </div>
                            );
                        })}
                    </div>
                </div>
            </ResponsiveDialog.Body>
            <ResponsiveDialog.Footer>
                <Button variant="outline-primary" onClick={onClose}>Close</Button>
                <Button variant="primary" onClick={submitClick}>Reprocess</Button>
            </ResponsiveDialog.Footer>
        </ResponsiveDialog >
    );
};

ReprocessModal.displayName = "ReprocessModal";


interface ReprocessModalProps {
    instrumentId: string;
    onClose: () => void;
}
