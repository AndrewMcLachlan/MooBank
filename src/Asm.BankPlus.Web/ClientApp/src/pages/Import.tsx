import React from "react";
import { usePageTitle } from "../hooks";
import { RouteComponentProps, useHistory, useParams } from "react-router-dom";
import { AccountController } from "../models";
import { Upload, FilesAddedEvent, PageHeader } from "../components";
import { useAccount, useImportTransactions } from "../services";
import Button from "react-bootstrap/Button";
import { useState } from "react";

export const Import: React.FC<RouteComponentProps<{ id: string }>> = (props) => {

    usePageTitle("Import Transactions");

    const history = useHistory();

    const { id } = useParams<any>();
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
                history.goBack();
            }
        });
    }

    return (
        <>
            <PageHeader title="Import Transactions" breadcrumbs={[[account.data.name, `/accounts/${id}`], ["Import", `/accounts/${id}/import`]]} />
            <Upload onFilesAdded={filesAdded} />
            <Button variant="primary" onClick={submitClick} disabled={!file}>Import</Button>
        </>
    );
}