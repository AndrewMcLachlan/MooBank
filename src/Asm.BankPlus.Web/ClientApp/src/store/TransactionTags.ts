import { Dispatch } from "redux";

import * as Models from "models";
import { ActionWithData } from "./redux-extensions";
import { TransactionTags, State } from "./state";
import { TransactionTagService } from "services/TransactionTagService";

const RequestTags = "RequestTags";
const ReceiveTags = "ReceiveTags";
const CreateTag = "CreateTransactionTag";

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

        const tags = await service.getTags();

        dispatch({ type: ReceiveTags, data: tags });
    },

    createTag: (name: string) => async (dispatch: Dispatch, getState: () => State) => {

        const service = new TransactionTagService(getState());

        const tag = await service.createTag(name);

        dispatch({type: CreateTag, data: tag});

        return tag;
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
                tags: (action.data as Models.TransactionTag[]).sort((t1,t2) => t1.name.localeCompare(t2.name)),
                areLoading: false,
            };

        case CreateTag:
            state.tags.push((action.data as Models.TransactionTag));
            state.tags.sort((t1,t2) => t1.name.localeCompare(t2.name));
            return state;
    }
    return state;
};
