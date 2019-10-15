﻿import { Dispatch } from "redux";

import * as Models from "models";
import { ActionWithData } from "./redux-extensions";
import { TransactionTags, State } from "./state";
import { TransactionTagService } from "services/TransactionTagService";
import { genericCaller, ShowMessage } from "./App";

const RequestTags = "RequestTags";
const ReceiveTags = "ReceiveTags";
const CreateTag = "CreateTransactionTag";
const AddTransactionTag = "TagAddTransactionTag";
const RemoveTransactionTag = "TagRemoveTransactionTag";

export const initialState: TransactionTags = {
    tags: [],
    areLoading: false,
};

export const actionCreators = {
    requestTags: () => async (dispatch: Dispatch, getState: () => State) => {

        const state = getState();

        if (state.transactionTags.areLoading) {
            // Don't issue a duplicate request (we already have or are loading the requested data)
            return;
        }

        dispatch({ type: RequestTags });

        const service = new TransactionTagService(state);

        try {
            const tags = await service.getTags();
            dispatch({ type: ReceiveTags, data: tags });
        }
        catch (error) {
            dispatch({ type: ShowMessage, data: error.message });
            dispatch({ type: ReceiveTags, data: [] });
        }
    },

    createTag: (name: string) => async (dispatch: Dispatch, getState: () => State) => {

        const service = new TransactionTagService(getState());

        try {
            const tag = await service.createTag(name);
            dispatch({ type: CreateTag, data: tag });
        }
        catch (error) {
            dispatch({ type: ShowMessage, data: error.message });
        }
    },

    addTransactionTag: (tagId: number, subId: number) => async (dispatch: Dispatch, getState: () => State) => {

        const state = getState();

        const service = new TransactionTagService(state);

        try {
            const transaction = await service.addTransactionTag(tagId, subId);
            dispatch({ type: AddTransactionTag, data: transaction });
        }
        catch (error) {
            dispatch({ type: ShowMessage, data: error.message });
        }
    },

    removeTransactionTag: (tagId: number, subId: number) => async (dispatch: Dispatch, getState: () => State) => {

        const service = new TransactionTagService(getState());

        try {
            const transaction = await service.removeTransactionTag(tagId, subId);
            dispatch({ type: RemoveTransactionTag, data: transaction });
        }
        catch (error) {
            dispatch({ type: ShowMessage, data: error.message });
        }
    },

    createTagAndAdd: (tagId: number, tagName: string) => async (dispatch: Dispatch, getState: () => State) => {
        const state = getState();

        const service = new TransactionTagService(state);

        const tag = await service.createTag(tagName);

        try {
            const transaction = await service.addTransactionTag(tagId, tag.id);
            dispatch({ type: AddTransactionTag, data: transaction });
        }
        catch (error) {
            dispatch({ type: ShowMessage, data: error.message });
        }
    },
};

export const reducer = (state: TransactionTags = initialState, action: ActionWithData<Models.TransactionTag[] | Models.TransactionTag>): TransactionTags => {

    switch (action.type) {
        case RequestTags:
            return {
                ...state,
                areLoading: true,
            };


        case ReceiveTags:

            return {
                ...state,
                tags: (action.data as Models.TransactionTag[]).sort((t1, t2) => t1.name.localeCompare(t2.name)),
                areLoading: false,
            };

        case CreateTag:
            state.tags.push((action.data as Models.TransactionTag));
            state.tags.sort((t1, t2) => t1.name.localeCompare(t2.name));
            return state;
    }
    return state;
};
