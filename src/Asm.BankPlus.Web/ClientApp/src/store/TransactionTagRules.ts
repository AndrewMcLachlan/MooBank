import { Dispatch } from "redux";

import * as Models from "../models";
import { ActionWithData } from "./redux-extensions";
import { TransactionTagRules, State } from "./state";
import { TransactionTagService, TransactionTagRuleService } from "../services";
import { ShowMessage } from "./App";

const RequestRules = "RequestRules";
const ReceiveRules = "ReceiveRules";
const CreateRule = "CreateTransactionTagRule";
const DeleteRule= "DeleteTransactionTagRule";
const AddTransactionTag = "RuleAddTransactionTag";
const RemoveTransactionTag = "RuleRemoveTransactionTag";

export const initialState: TransactionTagRules = {
    rules: [],
    areLoading: false,
};

export const actionCreators = {
    requestRules: () => async (dispatch: Dispatch, getState: () => State) => {

        const state = getState();

        if (!state.accounts.selectedAccount) return;

        if (state.transactionTagRules.areLoading) {
            // Don't issue a duplicate request (we already have or are loading the requested data)
            return;
        }

        dispatch({ type: RequestRules });

        const service = new TransactionTagRuleService(state);

        try {
            const rules = await service.getRules(state.accounts.selectedAccount.id);
            dispatch({ type: ReceiveRules, data: rules.rules });
        }
        catch (error) {
            console.error(error.message);
            dispatch({ type: ShowMessage, data: error.message });
            dispatch({ type: ReceiveRules, data: [] });
        }
    },

    createRule: (rule: Models.TransactionTagRule) => async (dispatch: Dispatch, getState: () => State) => {

        const state = getState();

        const service = new TransactionTagRuleService(state);

        try {
            const newRule = await service.createRule(state.accounts.selectedAccount.id, rule);
            dispatch({ type: CreateRule, data: newRule });
        }
        catch (error) {
            console.error(error.message);
            dispatch({ type: ShowMessage, data: error.message });
        }
    },

    deleteRule: (rule: Models.TransactionTagRule) => async (dispatch: Dispatch, getState: () => State) => {
        const state = getState();

        const service = new TransactionTagRuleService(state);

        try {
            await service.deleteRule(state.accounts.selectedAccount.id, rule.id);
            dispatch({ type: DeleteRule, data: rule });
        }
        catch (error) {
            console.error(error.message);
            dispatch({ type: ShowMessage, data: error.message });
        }
    },

    addTransactionTag: (ruleId: number, tagId: number) => async (dispatch: Dispatch, getState: () => State) => {

        const state = getState();

        const service = new TransactionTagRuleService(state);

        try {
            const transaction = await service.addTransactionTag(state.accounts.selectedAccount.id, ruleId, tagId);
            dispatch({ type: AddTransactionTag, data: transaction });
        }
        catch (error) {
            dispatch({ type: ShowMessage, data: error.message });
        }
    },

    removeTransactionTag: (ruleId: number, tagId: number) => async (dispatch: Dispatch, getState: () => State) => {

        const state = getState();

        const service = new TransactionTagRuleService(state);

        try {
            const transaction = await service.removeTransactionTag(state.accounts.selectedAccount.id, ruleId, tagId);
            dispatch({ type: RemoveTransactionTag, data: transaction });
        }
        catch (error) {
            dispatch({ type: ShowMessage, data: error.message });
        }
    },

    createTagAndAdd: (ruleId: number, tagName: string) => async (dispatch: Dispatch, getState: () => State) => {
        const state = getState();

        const transactionTagService = new TransactionTagService(state);

        const tag = await transactionTagService.createTag(tagName);

        const service = new TransactionTagRuleService(state);

        try {
            const transaction = await service.addTransactionTag(state.accounts.selectedAccount.id, ruleId, tag.id);
            dispatch({ type: AddTransactionTag, data: transaction });
        }
        catch (error) {
            dispatch({ type: ShowMessage, data: error.message });
        }
    },

    runRules: () => async (dispatch: Dispatch, getState: () => State) => {

        const state = getState();

        const service = new TransactionTagRuleService(state);

        try {
            await service.runRules(state.accounts.selectedAccount.id);
        }
        catch (error) {
            console.error(error.message);
            dispatch({ type: ShowMessage, data: error.message });
        }
    },
};

export const reducer = (state: TransactionTagRules = initialState, action: ActionWithData<Models.TransactionTagRule[] | Models.TransactionTagRule>): TransactionTagRules => {

    switch (action.type) {
        case RequestRules:
            return {
                ...state,
                areLoading: true,
            };


        case ReceiveRules:

            return {
                ...state,
                rules: (action.data as Models.TransactionTagRule[]).sort((t1, t2) => t1.contains.localeCompare(t2.contains)),
                areLoading: false,
            };

        case CreateRule:
            const newRules = [ ...state.rules, (action.data as Models.TransactionTagRule)];
            newRules.sort((t1, t2) => t1.contains.localeCompare(t2.contains));
            return {
                ...state,
                rules: newRules,
            };

        case DeleteRule:
            state.rules = state.rules.filter(r => r.id !== (action.data as Models.TransactionTagRule).id)
            state.rules.sort((t1, t2) => t1.contains.localeCompare(t2.contains));
            return state;
    }
    return state;
};
