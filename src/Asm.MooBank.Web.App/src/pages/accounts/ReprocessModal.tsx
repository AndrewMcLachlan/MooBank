import React, { useEffect } from "react";
import { useState } from "react";
import { Button, Modal } from "@andrewmclachlan/moo-ds";
import { useAccount } from "components";
import { LogicalAccount } from "models";
import { useReprocessTransactions } from "services";

export const ReprocessModal: React.FC<ReprocessModalProps> = ({ show, instrumentId, onClose }) => {

    const account = useAccount() as LogicalAccount;

    const reprocessTransactions = useReprocessTransactions();

    const openAccounts = account.institutionAccounts.filter(ia => ia.closedDate === null);

    const [institutionAccountId, setInstitutionAccountId] = useState<string | null>(openAccounts[0]?.id ?? null);

    useEffect(() => {
        setInstitutionAccountId(openAccounts[0]?.id ?? null);
    }, [show, account]);

    if (account?.controller !== "Import") {
        return null;
    }

    const submitClick = () => {
        if (!institutionAccountId) return;
        reprocessTransactions(instrumentId, institutionAccountId);
        onClose();
    }

    return (
        <Modal className="import" show={show} onHide={onClose} size="lg">
            <Modal.Header closeButton>
                <Modal.Title>Reprocess Imported Transactions</Modal.Title>
            </Modal.Header>
            <Modal.Body>
                <div>
                    <div className="import-types" hidden={openAccounts.length <= 1}>
                        {openAccounts.map(ia => {
                            return (
                                <div key={ia.id} className={`import-type-option ${institutionAccountId === ia.id ? 'selected' : ''}`} onClick={() => setInstitutionAccountId(ia.id)}>
                                    <input type="radio" id={ia.id} name="institutionAccountId" value={ia.id} className="form-check-input" checked={institutionAccountId === ia.id} onChange={(e) => { setInstitutionAccountId(ia.id); }} />
                                    <label htmlFor={ia.id} className="form-label">{ia.name ?? ia.institutionId}</label>
                                </div>
                            );
                        })}
                    </div>
                </div>
            </Modal.Body>
            <Modal.Footer>
                <Button variant="outline-primary" onClick={onClose}>Close</Button>
                <Button variant="primary" onClick={submitClick}>Reprocess</Button>
            </Modal.Footer>
        </Modal >
    );
};

ReprocessModal.displayName = "ReprocessModal";


interface ReprocessModalProps {
    show: boolean;
    instrumentId: string;
    onClose: () => void;
}
