import Msal from "msal";

import { ActionWithData } from "./redux-extensions";
import { Security } from "./state";

const loggedIn = "LoggedIn";
const loggedOut = "LoggedOut";
const initialState: Security = { loggedIn: false, msal: null };

export const actionCreators = {
    logIn: () => ({ type: loggedIn }),
    logOut: () => ({ type: loggedOut })
};

export const reducer = (state: Security, action: ActionWithData<Msal.UserAgentApplication>) => {
    state = state || initialState;

    if (action.type === loggedIn) {
        return { ...state, loggedIn: true, msal: action.data };
    }

    if (action.type === loggedOut) {
        return { ...state, loggedOut: false, msal: null };
    }

    return state;
};
