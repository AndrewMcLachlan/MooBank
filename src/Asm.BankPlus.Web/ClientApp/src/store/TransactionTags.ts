import { Dispatch } from "redux";

import HttpClient from "../services/HttpClient";
import { ActionWithData } from "./redux-extensions";
import { TransactionTags, State } from "./state";
import * as Models from "../models";

const RequestCategories = "RequestCategories";
const ReceiveCategories = "ReceiveCategories";
export const CreateTransactionTag = "CreateTransactionTag";

export const initialState: TransactionTags = {
    tags: [],
    areLoading: false,
};

export const actionCreators = {
    requestCategories: () => async (dispatch: Dispatch, getState: () => State) => {

        const state = getState();

        if (state.accounts.areLoading) {
            // Don't issue a duplicate request (we already have or are loading the requested data)
            return;
        }

        dispatch({ type: RequestCategories });

        const url = `api/transaction/tags`;

        const client = new HttpClient(state.app.baseUrl);

        const tags = await client.get<Models.TransactionTag[]>(url);

        dispatch({ type: ReceiveCategories, data: tags });
    },

    createTag: (name: string) => (dispatch: Dispatch, getState: () => State) => {

        const state = getState();

        const url = `api/transaction/tags/${name}`;

        const client = new HttpClient(state.app.baseUrl);

        const tag = client.put<undefined, Models.TransactionTag>(url);

        return tag;
    },
};

export const reducer = (state: TransactionTags = initialState, action: ActionWithData<any>): TransactionTags => {

    switch (action.type) {
        case RequestCategories:
            return {
                ...state,
                areLoading: true,
            };


        case ReceiveCategories:

            return {
                ...state,
                tags: action.data,
                areLoading: false,
            };

        case CreateTransactionTag:
            state.tags.push(action.data);
            return state;
    }
    return state;
};
