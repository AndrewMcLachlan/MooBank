import { App } from "./state";

const initialState: App = {
    appName: "",
    baseUrl: "",
    skin: "",
};

export const reducer = (state: App, action:any) => {
    state = state || initialState;

    return state;
};
