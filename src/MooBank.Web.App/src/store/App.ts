import type { App } from "./state";
import { createSlice } from "@reduxjs/toolkit";
import type { PayloadAction } from "@reduxjs/toolkit";

export const initialState: App = {
    baseUrl: "/",
};

export const reducers = {
    showMessage: (state: App, action: PayloadAction<string>) => {
            return {
                ...state,
                message: action.payload,
            }
    }
}

export const AppSlice = createSlice({
    name: "App",
    initialState: initialState,
    reducers: reducers
});
