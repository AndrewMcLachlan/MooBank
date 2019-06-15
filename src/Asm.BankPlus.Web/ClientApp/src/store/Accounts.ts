import { Dispatch } from "redux";

import HttpClient from "../HttpClient";
import { ActionWithData } from "./redux-extensions";
import { Accounts, State } from "./state";

const RequestAccounts = "RequestAccounts";
const ReceiveAccounts = "ReceiveAccounts";

const initialState: Accounts = {
    accounts: [],
    areLoading: false,
    virtualAccounts: [],
};

export const actionCreators = {
    requestAccounts: () => async (dispatch: Dispatch, getState: () => State) => {

        const state = getState();

        if (state.accounts.areLoading) {
            // Don"t issue a duplicate request (we already have or are loading the requested data)
            return;
        }

        dispatch({ type: RequestAccounts });

        const url = `api/accounts`;

        const client = new HttpClient(state.app.baseUrl, state.security.msal);

        const accounts = await client.get<Accounts>(url);

        dispatch({ type: ReceiveAccounts, data: accounts });
    },
};

export const reducer = (state: Accounts = initialState, action: ActionWithData<Accounts>): Accounts => {

    if (action.type === RequestAccounts) {
        return {
            ...state,
            areLoading: true,
        };
    }

    if (action.type === ReceiveAccounts) {
        return {
            ...state,
            accounts: action.data.accounts,
            areLoading: false,
            virtualAccounts: action.data.virtualAccounts,
        };
    }

    return state;
};
