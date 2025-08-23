import React from "react";
import { Upload, FilesAddedEvent } from "@andrewmclachlan/moo-ds";
import { useImportTransactions } from "../../../services";
import Button from "react-bootstrap/Button";
import { useState } from "react";
import { Modal } from "react-bootstrap";
import { useAccount } from "components";

export const Import: React.FC<ImportProps> = ({ show, accountId, onClose }) => {

    const account = useAccount();

    const importTransactions = useImportTransactions();

    const [file, setFile] = useState<File>();

    if (account?.controller !== "Import") {
        return null;
    }

    const filesAdded = (e: FilesAddedEvent) => {
        setFile(e.newFiles[0]);
    }

    const submitClick = () => {
        if (!file) return;
        importTransactions(accountId, file, {
            onSuccess: () => {
                onClose();
            }
        });
    }

    return (
        <Modal className="import" show={show} onHide={onClose} size="lg" centered>
            <Modal.Header closeButton>
                <Modal.Title>Import Transactions</Modal.Title>
            </Modal.Header>
            <Modal.Body>
                <Upload onFilesAdded={filesAdded} accept="text/csv" />
            </Modal.Body>
            <Modal.Footer>
                <Button variant="outline-primary" onClick={onClose}>Close</Button>
                <Button variant="primary" onClick={submitClick} disabled={!file}>Import</Button>
            </Modal.Footer>
        </Modal>
    );
};

Import.displayName = "Import";


interface ImportProps {
    show: boolean;
    accountId: string;
    onClose: () => void;
}
