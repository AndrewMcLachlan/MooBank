import { useSelector, useDispatch } from "react-redux";
import { bindActionCreators } from "redux";
import { useEffect } from "react";

import { actionCreators } from "store/Accounts";
import { State } from "store/state";

export const useSelectedAccount = (accountId: string)  => {

    const account = useSelector((state: State) => (state.accounts.selectedAccount && state.accounts.selectedAccount.id === accountId) ? state.accounts.selectedAccount : undefined);

    const dispatch = useDispatch();

    bindActionCreators(actionCreators, dispatch);

    useEffect(() => {
        if (!account || account.id !== accountId)
        {
            dispatch(actionCreators.requestAccount(accountId));
        } 
    }, [dispatch, account, accountId]);

    return account;
}