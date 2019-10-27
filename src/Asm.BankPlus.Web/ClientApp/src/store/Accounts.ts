﻿import { Dispatch } from "redux";

import HttpClient from "services/HttpClient";
import { ActionWithData } from "./redux-extensions";
import { Accounts, State } from "./state";
import { Account, ImportAccount } from "models";
import { ImportService } from "services";

export const ActionTypes = {
    CreateAccount: "CreateAccount",
    AccountCreated: "AccountCreated",
    RequestAccounts: "RequestAccounts",
    ReceiveAccounts: "ReceiveAccounts",
    RequestAccount: "RequestAccount",
    ReceiveAccount: "ReceiveAccount",
    SetSelectedAccount: "SetSelectedAccount",
    ImportTransactions: "ImportTransactions",
};

export const initialState: Accounts = {
    accounts: [],
    areLoading: false,
    virtualAccounts: [],
    selectedAccount: null,
    position: 0,
};

export const actionCreators = {
    requestAccounts: () => async (dispatch: Dispatch, getState: () => State) => {

        const state = getState();

        if (state.accounts.areLoading) {
            // Don"t issue a duplicate request (we already have or are loading the requested data)
            return;
        }

        dispatch({ type: ActionTypes.RequestAccounts });

        const url = `api/accounts`;

        const client = new HttpClient(state.app.baseUrl);

        const accounts = await client.get<Accounts>(url);

        dispatch({ type: ActionTypes.ReceiveAccounts, data: accounts });
    },

    requestAccount: (id: string) => async (dispatch: Dispatch, getState: () => State) => {

        const state = getState();

        dispatch({ type: ActionTypes.RequestAccount });

        const url = `api/accounts/${id}`;

        const client = new HttpClient(state.app.baseUrl);

        const account = await client.get<Account>(url);

        dispatch({ type: ActionTypes.ReceiveAccount, data: account });
    },

    createAccount: (account: Account, importAccount: ImportAccount) => async (dispatch: Dispatch, getState: () => State) => {
        const state = getState();

        dispatch({ type: ActionTypes.CreateAccount });

        const url = `api/accounts`;

        const client = new HttpClient(state.app.baseUrl);

        const newAccount = await client.post<{ account: Account, importAccount: ImportAccount }, Account>(url, { account, importAccount });

        dispatch({ type: ActionTypes.AccountCreated, data: newAccount })
    },

    importTransactions: (id: string, file: File) => async (dispatch: Dispatch, getState: () => State) => {

        const service = new ImportService(getState());

        await service.importTransactions(id, file);
    },
};

export const reducer = (state: Accounts = initialState, action: ActionWithData<any>): Accounts => {

    switch (action.type) {
        case ActionTypes.RequestAccounts:
            return {
                ...state,
                areLoading: true,
            };

        case ActionTypes.ReceiveAccounts:
            return {
                ...state,
                accounts: action.data.accounts,
                areLoading: false,
                virtualAccounts: action.data.virtualAccounts,
                position: action.data.position,
            };

        case ActionTypes.ReceiveAccount:
        case ActionTypes.SetSelectedAccount:
            return {
                ...state,
                selectedAccount: action.data,
            };

        case ActionTypes.AccountCreated:
            return {
                ...state,
                accounts: [...state.accounts, action.data],
            }


    }

    return state;
};
