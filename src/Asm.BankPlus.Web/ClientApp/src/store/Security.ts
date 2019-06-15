import { UserAgentApplication } from "msal";

import { ActionWithData } from "./redux-extensions";
import { Security } from "./state";

const loggedIn = "LoggedIn";
const loggedOut = "LoggedOut";
const initialState: Security = { loggedIn: false, msal: null, name: null };

export const actionCreators = {
    login: (name) => ({ type: loggedIn, data: name }),
    logout: () => ({ type: loggedOut })
};

export const reducer = (state: Security, action: ActionWithData<string>): Security => {
    state = state || initialState;

    if (action.type === loggedIn) {
        return { ...state, loggedIn: true, name: action.data };
    }

    if (action.type === loggedOut) {
        return { ...state, loggedIn: false, name: null };
    }

    return state;
};
