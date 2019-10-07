import { Dispatch } from "redux";

import HttpClient from "../services/HttpClient";
import { ActionWithData } from "./redux-extensions";
import { Transactions, State } from "./state";
import * as Models from "../models";
import { CreateTransactionTag } from "./TransactionTags";

const RequestTransactions = "RequestTransactions";
const ReceiveTransactions = "ReceiveTransactions";

const AddTransactionTag = "AddTransactionTag";
const RemoveTransactionTag = "RemoveTransactionTag";

const initialState: Transactions = {
    transactions: [],
    areLoading: false,
    currentPage: 1,
};

export const actionCreators = {
    requestTransactions: (accountId: string, pageNumber: number) => async (dispatch: Dispatch, getState: () => State) => {

        const state = getState();

        if (state.transactions.areLoading) {
            // Don't issue a duplicate request (we already have or are loading the requested data)
            return;
        }

        dispatch({ type: RequestTransactions });

        const url = `api/accounts/${accountId}/transactions/${pageNumber}`;

        const client = new HttpClient(state.app.baseUrl);

        const transactions = await client.get<Transactions>(url);

        transactions.currentPage = pageNumber;

        dispatch({ type: ReceiveTransactions, data: transactions });
    },

    addTransactionTag: (transactionId: string, tagId: number) => async (dispatch: Dispatch, getState: () => State) => {

        const state = getState();

        const url = `api/transactions/${transactionId}/tag/${tagId}`;

        const client = new HttpClient(state.app.baseUrl);

        await client.put(url);
    },

    removeTransactionTag: (transactionId: string, tagId: number) => async (dispatch: Dispatch, getState: () => State) => {

        const state = getState();

        const url = `api/transactions/${transactionId}/tag/${tagId}`;

        const client = new HttpClient(state.app.baseUrl);

        await client.delete(url);
    },

    createTagAndAdd: (transactionId: string, tagName: string) => async (dispatch: Dispatch, getState: () => State) => {
        const state = getState();

        let url = `api/transaction/tags/${tagName}`;

        const client = new HttpClient(state.app.baseUrl);

        const tag = await client.put<undefined, Models.TransactionTag>(url);

        dispatch({type: CreateTransactionTag, data: tag});

        url = `api/transactions/${transactionId}/tag/${tag.id}`;

        const transaction = await client.put<undefined, Models.Transaction>(url);

        dispatch({type: AddTransactionTag, data: transaction});
    },
};

export const reducer = (state: Transactions = initialState, action: ActionWithData<any>): Transactions => {

    switch (action.type) {

        case RequestTransactions:
            return {
                ...state,
                areLoading: true,
            };


        case ReceiveTransactions:
            return {
                ...state,
                transactions: action.data.transactions,
                currentPage: action.data.currentPage,
                areLoading: false,
            };

            case AddTransactionTag:
                const index = state.transactions.findIndex((t) => t.id === action.data.id);
                const transactions = [...state.transactions.slice(0, index), action.data, ...state.transactions.slice(index+1) ];
                return {
                    ...state,
                    transactions: transactions,
                };
    }

    return state;
};
