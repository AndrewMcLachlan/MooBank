import { Dispatch } from "redux";

import { ActionWithData } from "./redux-extensions";
import { Transactions, State } from "./state";

import { TransactionService, TransactionTagService } from "services";
import { ShowMessage } from "./App";

const RequestTransactions = "RequestTransactions";
const ReceiveTransactions = "ReceiveTransactions";

const AddTransactionTag = "TransactionAddTransactionTag";
const RemoveTransactionTag = "TransactionRemoveTransactionTag";

const initialState: Transactions = {
    transactions: [],
    areLoading: false,
    currentPage: 1,
    pageSize: 50,
    total: 0,
};

export const actionCreators = {
    requestTransactions: (accountId: string, pageNumber: number) => async (dispatch: Dispatch, getState: () => State) => {

        const state = getState();

        if (state.transactions.areLoading) {
            // Don't issue a duplicate request (we already have or are loading the requested data)
            return;
        }

        dispatch({ type: RequestTransactions });

        const service = new TransactionService(state);

        try {
            const transactions = await service.getTransactions(accountId, state.transactions.pageSize, pageNumber);
            dispatch({ type: ReceiveTransactions, data: transactions });
        }
        catch (error) {
            dispatch({ type: ShowMessage, data: error.message });
            dispatch({ type: ReceiveTransactions, data: [] });
        }
    },

    addTransactionTag: (transactionId: string, tagId: number) => async (dispatch: Dispatch, getState: () => State) => {

        const state = getState();

        const service = new TransactionService(state);

        try {
            const transaction = await service.addTransactionTag(transactionId, tagId);
            dispatch({ type: AddTransactionTag, data: transaction });
        }
        catch (error) {
            dispatch({ type: ShowMessage, data: error.message });
        }
    },

    removeTransactionTag: (transactionId: string, tagId: number) => async (dispatch: Dispatch, getState: () => State) => {

        const service = new TransactionService(getState());

        try {
            const transaction = await service.removeTransactionTag(transactionId, tagId);
            dispatch({ type: RemoveTransactionTag, data: transaction });
        }
        catch (error) {
            dispatch({ type: ShowMessage, data: error.message });
        }
    },

    createTagAndAdd: (transactionId: string, tagName: string) => async (dispatch: Dispatch, getState: () => State) => {
        const state = getState();

        const transactionTagService = new TransactionTagService(state);

        const tag = await transactionTagService.createTag(tagName, []);

        const service = new TransactionService(state);

        try {
            const transaction = await service.addTransactionTag(transactionId, tag.id);
            dispatch({ type: AddTransactionTag, data: transaction });
        }
        catch (error) {
            dispatch({ type: ShowMessage, data: error.message });
        }
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
                total: action.data.total,
                areLoading: false,
            };

        case AddTransactionTag: {
            const index = state.transactions.findIndex((t) => t.id === action.data.id);
            const transactions = [...state.transactions.slice(0, index), action.data, ...state.transactions.slice(index + 1)];
            return {
                ...state,
                transactions: transactions,
            };
        }
        case RemoveTransactionTag: {
            const index = state.transactions.findIndex((t) => t.id === action.data.id);
            const transactions = [...state.transactions.slice(0, index), action.data, ...state.transactions.slice(index + 1)];
            return {
                ...state,
                transactions: transactions,
            };
        }
    }

    return state;
};
