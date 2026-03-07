import React from "react";
import { Button, Upload, FilesAddedEvent } from "@andrewmclachlan/moo-ds";
import { useImportTransactions } from "routes/accounts/-hooks/useImportTransactions";
import { useState } from "react";
import { Modal } from "@andrewmclachlan/moo-ds";
import { useAccount } from "components";
import type { LogicalAccount } from "api/types.gen";

export const Import: React.FC<ImportProps> = ({ show, accountId, onClose }) => {

    const account = useAccount() as LogicalAccount;

    const importTransactions = useImportTransactions();

    const [file, setFile] = useState<File>();

    const openAccounts = account.institutionAccounts?.filter(ia => ia.closedDate === null);

    const [institutionAccountId, setInstitutionAccountId] = useState<string | null>(openAccounts?.[0]?.id ?? null);

    if (account?.controller !== "Import") {
        return null;
    }

    const filesAdded = (e: FilesAddedEvent) => {
        setFile(e.newFiles[0]);
    }

    const submitClick = () => {
        if (!file) return;
        importTransactions(accountId, institutionAccountId, file, {
            onSuccess: () => {
                onClose();
            }
        });
    }

    return (
        <Modal className="import" show={show} onHide={onClose} size="lg">
            <Modal.Header closeButton>
                <Modal.Title>Import Transactions</Modal.Title>
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
                    <Upload onFilesAdded={filesAdded} accept="text/csv" />
                </div>
            </Modal.Body>
            <Modal.Footer>
                <Button variant="outline-primary" onClick={onClose}>Close</Button>
                <Button variant="primary" onClick={submitClick} disabled={!file}>Import</Button>
            </Modal.Footer>
        </Modal >
    );
};

Import.displayName = "Import";


interface ImportProps {
    show: boolean;
    accountId: string;
    onClose: () => void;
}
