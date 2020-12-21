import React from "react";
import { usePageTitle } from "../hooks";
import { RouteComponentProps, useParams } from "react-router-dom";
import { AccountController } from "../models";
import { Upload, FilesAddedEvent } from "../components";
import { useDispatch } from "react-redux";
import { bindActionCreators } from "redux";
import { actionCreators } from "../store/Accounts";
import { useAccount } from "../services";

export const Import: React.FC<RouteComponentProps<{ id: string }>> = (props) => {

    usePageTitle("Import Transactions");

    const { id } = useParams<any>();
    const account = useAccount(id);
    const dispatch = useDispatch();
    bindActionCreators(actionCreators, dispatch);

    if (!account.data || account.data?.controller !== AccountController.Import) {
        return null;
    }

    const filesAdded = (e: FilesAddedEvent) => {
        dispatch(actionCreators.importTransactions(id, e.newFiles[0]));
    }

    return <Upload onFilesAdded={filesAdded} />;
}