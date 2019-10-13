import React from "react";
import { useSelectedAccount } from "hooks";
import { RouteComponentProps } from "react-router-dom";
import { AccountController } from "models";
import { Upload, FilesAddedEvent } from "components";
import { useDispatch } from "react-redux";
import { bindActionCreators } from "redux";
import { actionCreators } from "store/Accounts";

export const Import: React.FC<RouteComponentProps<{ id: string }>> = (props) => {

    const accountId = props.match.params.id;
    const account = useSelectedAccount(accountId);
    const dispatch = useDispatch();
    bindActionCreators(actionCreators, dispatch);

    if (!account || account.controller !== AccountController.Import) {
        return null;
    }

    const filesAdded = (e:FilesAddedEvent) => {
        dispatch(actionCreators.importTransactions(accountId, e.newFiles[0]));
    }

    return <Upload onFilesAdded={filesAdded} />;
}