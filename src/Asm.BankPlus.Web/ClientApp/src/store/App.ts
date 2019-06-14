import { App } from "./state";

const initialState: App = {
    appName: "",
    baseUrl: "",
    skin: "",
};

export const reducer = (state, action) => {
    state = state || initialState;

    return state;
};
