import { Dispatch } from "redux";

import { ActionWithData } from "./redux-extensions";
import { State, ReferenceData } from "./state";
import { ReferenceDataService } from "../services";
import { ShowMessage } from "./App";

export const ActionTypes = {
    RequestImporterTypes: "RequestImporterTypes",
    ReceiveImporterTypes: "ReceiveImporterTypes",
};

export const initialState: ReferenceData = {
    importAccountTypes: [],
};

export const actionCreators = {
    requestImporterTypes: () => async (dispatch: Dispatch, getState: () => State) => {

        const state = getState();

        dispatch({ type: ActionTypes.RequestImporterTypes });

        const service = new ReferenceDataService(state);

        try {
            const types = await service.getImporterTypes();
            dispatch({ type: ActionTypes.ReceiveImporterTypes, data: types });
        }
        catch (error) {
            console.error(error.message);
            dispatch({ type: ShowMessage, data: error.message });
        }        
    },
};

export const reducer = (state: ReferenceData = initialState, action: ActionWithData<any>): ReferenceData => {

    switch (action.type) {
        case ActionTypes.ReceiveImporterTypes:
            return {
                ...state,
                importAccountTypes: action.data,
            };
    }

    return state;
};
