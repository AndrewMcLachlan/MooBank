import React from "react";
import { useNavigate } from "react-router-dom";
import { AccountController } from "models";
import { Upload, FilesAddedEvent } from "@andrewmclachlan/mooapp";
import { useAccount, useImportTransactions } from "services";
import Button from "react-bootstrap/Button";
import { useState } from "react";
import { Page } from "layouts";
import { useIdParams } from "hooks";

export const Import: React.FC = () => {

    const navigate = useNavigate();

    const id = useIdParams();

    const account = useAccount(id);

    const importTransactions = useImportTransactions();

    const [file, setFile] = useState<File>();

    if (!account.data || account.data?.controller !== AccountController.Import) {
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
        <Page title="Import Transactions">
            <Page.Header title="Import Transactions" breadcrumbs={[[account.data.name, `/accounts/${id}`], ["Import", `/accounts/${id}/import`]]} goBack />
            <Page.Content>
                <Upload onFilesAdded={filesAdded} accept="text/csv" />
                <Button variant="primary" onClick={submitClick} disabled={!file}>Import</Button>
            </Page.Content>
        </Page>
    );
}