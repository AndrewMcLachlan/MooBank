import React from "react";
import { useNavigate } from "react-router-dom";
import { Upload, useIdParams, FilesAddedEvent, Section } from "@andrewmclachlan/mooapp";
import { useAccount, useImportTransactions } from "../services";
import Button from "react-bootstrap/Button";
import { useState } from "react";
import { AccountPage } from "components";

export const Import: React.FC = () => {

    const navigate = useNavigate();

    const id = useIdParams();

    const account = useAccount(id);

    const importTransactions = useImportTransactions();

    const [file, setFile] = useState<File>();

    if (!account.data || account.data?.controller !== "Import") {
        return null;
    }

    const filesAdded = (e: FilesAddedEvent) => {
        setFile(e.newFiles[0]);
    }

    const submitClick = () => {
        if (!file) return;
        importTransactions.mutate({ accountId: id, file: file }, {
            onSuccess: () => {
                navigate(-1);
            }
        });
    }

    return (
        <AccountPage title="Import Transactions" breadcrumbs={[{ text: "Import", route: `/accounts/${id}/import` }]}>
            <Section>
                <Upload onFilesAdded={filesAdded} accept="text/csv" />
                <Button variant="primary" onClick={submitClick} disabled={!file}>Import</Button>
            </Section>
        </AccountPage>
    );
}