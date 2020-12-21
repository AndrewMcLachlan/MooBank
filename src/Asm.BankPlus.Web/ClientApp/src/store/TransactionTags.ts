import { Dispatch } from "redux";

import * as Models from "../models";
import { ActionWithData } from "./redux-extensions";
import { TransactionTags, State } from "./state";
import { TransactionTagService } from "../services/TransactionTagService";
import { ShowMessage } from "./App";

const RequestTags = "RequestTags";
const ReceiveTags = "ReceiveTags";
const CreateTag = "CreateTransactionTag";
const DeleteTag = "DeleteTransactionTag";
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
            console.error(error.message);
            dispatch({ type: ShowMessage, data: error.message });
            dispatch({ type: ReceiveTags, data: [] });
        }
    },

    createTag: (newTag: string | Models.TransactionTag) => async (dispatch: Dispatch, getState: () => State) => {

        const service = new TransactionTagService(getState());

        const name = ((newTag as Models.TransactionTag).name || (newTag as string)).trim();

        const tags = (newTag as Models.TransactionTag).tags || [];

        try {
            const tag = await service.createTag(name, tags);
            dispatch({ type: CreateTag, data: tag });
        }
        catch (error) {
            console.error(error.message);
            dispatch({ type: ShowMessage, data: error.message });
        }
    },

    deleteTag: (tag: Models.TransactionTag) => async (dispatch: Dispatch, getState: () => State) => {

        const service = new TransactionTagService(getState());

        try {
            await service.deleteTag(tag);
            dispatch({ type: DeleteTag, data: tag });
        }
        catch (error) {
            console.error(error.message);
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
            console.error(error.message);
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
            console.error(error.message);
            dispatch({ type: ShowMessage, data: error.message });
        }
    },

    createTagAndAdd: (tagId: number, tagName: string) => async (dispatch: Dispatch, getState: () => State) => {
        const state = getState();

        const service = new TransactionTagService(state);

        const tag = await service.createTag(tagName, []);

        try {
            const transaction = await service.addTransactionTag(tagId, tag.id);
            dispatch({ type: AddTransactionTag, data: transaction });
        }
        catch (error) {
            console.error(error.message);
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
            state.tags = [ ...state.tags, action.data as Models.TransactionTag];
            state.tags = state.tags.sort((t1, t2) => t1.name.localeCompare(t2.name));
            return state;

        case DeleteTag:
            state.tags = state.tags.filter(r => r.id !== (action.data as Models.TransactionTag).id)
            state.tags = state.tags.sort((t1, t2) => t1.name.localeCompare(t2.name));
            return state;
    }
    return state;
};
