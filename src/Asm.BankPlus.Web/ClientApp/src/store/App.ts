import { App } from "./state";
import { ActionWithData } from "./redux-extensions";
import { Dispatch } from "react";

const initialState: App = {
    appName: "",
    baseUrl: "",
    skin: "",
};

export const ShowMessage = "ShowMessage";

export const genericCaller = (dispatch: Dispatch<ActionWithData<string>>, callback: () => any) => {
    try {
        return callback();
    }
    catch (error) {
        dispatch({ type: "ShowMessage", data: (error as Error).message});
    }
}

export const reducer = (state: App, action:ActionWithData<string>) => {
    state = state || initialState;

    switch (action.type) {
        case "ShowMessage":
            return {
                ...state,
                message: action.data,
            }
    }

    return state;
};
