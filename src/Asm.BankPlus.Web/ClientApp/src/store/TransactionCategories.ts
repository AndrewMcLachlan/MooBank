import { Dispatch } from "redux";

import HttpClient from "../services/HttpClient";
import { ActionWithData } from "./redux-extensions";
import { TransactionCategories, State } from "./state";
import * as Models from "../models";

const RequestCategories = "RequestCategories";
const ReceiveCategories = "ReceiveCategories";

const initialState: TransactionCategories = {
    categories: [],
    areLoading: false,
};

export const actionCreators = {
    requestCategories: () => async (dispatch: Dispatch, getState: () => State) => {

        const state = getState();

        if (state.accounts.areLoading) {
            // Don"t issue a duplicate request (we already have or are loading the requested data)
            return;
        }

        dispatch({ type: RequestCategories });

        const url = `api/transaction/category`;

        const client = new HttpClient(state.app.baseUrl);

        const accounts = await client.get<Models.TransactionCategory[]>(url);

        dispatch({ type: ReceiveCategories, data: accounts });
    },
};

export const reducer = (state: TransactionCategories = initialState, action: ActionWithData<Models.TransactionCategory[]>): TransactionCategories => {

    if (action.type === RequestCategories) {
        return {
            ...state,
            areLoading: true,
        };
    }

    if (action.type === ReceiveCategories) {
        return {
            ...state,
            categories: action.data,
            areLoading: false,
        };
    }

    return state;
};
