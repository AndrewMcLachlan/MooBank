import { Dispatch } from "redux";

import { ActionWithData } from "./redux-extensions";
import { Transactions, State } from "./state";

export const SetTransactionListFilter = "SetTransactionListFilter";
export const SetCurrentPage = "SetCurrentPage";

const initialState: Transactions = {
    areLoading: false,
    currentPage: 1,
    pageSize: 50,
    total: 0,
    filterTagged: false,
};

export const reducer = (state: Transactions = initialState, action: ActionWithData<any>): Transactions => {

    switch (action.type) {

        case SetCurrentPage:
            return {
                ...state,
                currentPage: action.data
            };

        case SetTransactionListFilter: {
            return {
                ...state,
                filterTagged: action.data,
            }
        }
    }

    return state;
};
