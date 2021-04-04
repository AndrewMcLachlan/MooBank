import React from "react";
import { usePageTitle } from "../hooks";
import { RouteComponentProps, useParams } from "react-router-dom";
import { AccountController } from "../models";
import { Upload, FilesAddedEvent, PageHeader } from "../components";
import { useAccount, useImportTransactions } from "../services";

export const Import: React.FC<RouteComponentProps<{ id: string }>> = (props) => {

    usePageTitle("Import Transactions");

    const { id } = useParams<any>();
    const account = useAccount(id);

    const importTransactions = useImportTransactions();

    if (!account.data || account.data?.controller !== AccountController.Import) {
        return null;
    }

    const filesAdded = (e: FilesAddedEvent) => {
        importTransactions.mutate({ accountId: id, file: e.newFiles[0] });
    }

    return (
        <>
        <PageHeader title="Import Transactions" breadcrumbs={[[account.data.name,`/accounts/${id}`], ["Import", `/accounts/${id}/import`]]} />
        <Upload onFilesAdded={filesAdded} />
        </>
    );
}