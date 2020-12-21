import * as Models from "../models";
import { ServiceBase } from "./ServiceBase";
import HttpClient, { httpClient } from "./HttpClient";
import { useApiQuery } from "./useApiQuery";
import { useQueryClient } from "react-query";
import { useMutation } from "react-query";

export const useRules = (accountId: string) => useApiQuery<Models.TransactionTagRules>(["rules", accountId], `api/accounts/${accountId}/transaction/tag/rules`);

export const useRunRules = () => useMutation<null, null, { accountId: string }>(async (variables) => await httpClient.post(`api/accounts/${variables.accountId}/transaction/tag/rules/run`));

export const useAddTransactionTagRuleTag = () => {

    const queryClient = useQueryClient();

    return useMutation<Models.TransactionTagRule, null, { accountId: string, ruleId: number, tagId: number }>(async (variables) => (await httpClient.put<Models.TransactionTagRule>(`api/accounts/${variables.accountId}/transaction/tag/rules/${variables.ruleId}/tag/${variables.tagId}`)).data, {
        onSuccess: (data: Models.TransactionTagRule, variables: { accountId: string, ruleId: number, tagId: number }) => {
            queryClient.setQueryData<Models.TransactionTagRule>(["transactionrules", { id: variables.ruleId }], data);
        }
    });
}

export const useRemoveTransactionTagRuleTag = () => {

    const queryClient = useQueryClient();

    return useMutation<Models.TransactionTagRule, null, { accountId: string, ruleId: number, tagId: number }>(async (variables) => (await httpClient.delete<Models.TransactionTagRule>(`api/accounts/${variables.accountId}/transaction/tag/rules/${variables.ruleId}/tag/${variables.tagId}`)).data, {
        onSuccess: (data: Models.TransactionTagRule, variables: { accountId: string, ruleId: number, tagId: number }) => {
            queryClient.setQueryData<Models.TransactionTagRule>(["transactionrules", { id: variables.ruleId }], data);
        }
    });
}

export const useCreateRule = () => {

    const queryClient = useQueryClient();

    return useMutation<Models.TransactionTagRule, null, { accountId: string; rule: Models.TransactionTagRule }>(async (variables) => (await httpClient.post<Models.TransactionTagRule>(`api/accounts/${variables.accountId}/transaction/tag/rules`, variables.rule)).data, {
        onSuccess: (data: Models.TransactionTagRule) => {
            queryClient.setQueryData<Models.TransactionTagRule>(["tags", { id: data.id }], data);
            let allRules = queryClient.getQueryData<Models.TransactionTagRule[]>(["transactionrules"]);
            allRules = allRules.sort((t1, t2) => t1.contains.localeCompare(t2.contains));
            queryClient.setQueryData<Models.TransactionTagRule[]>(["transactionrules"], allRules);
        }
    });
}

export const useDeleteRule = () => {

    const queryClient = useQueryClient();

    return useMutation<null, null, { accountId: string; ruleId: number}>(async (variables) => (await httpClient.delete(`api/accounts/${variables.accountId}/transaction/tag/rules/${variables.ruleId}`)).data, {
        onSuccess: (variables: { accountId: string; ruleId: number}) => {
            let allTags = queryClient.getQueryData<Models.TransactionTagRule[]>(["transactionrules"]);
            allTags = allTags.filter(r => r.id !== (variables.ruleId));
            allTags = allTags.sort((t1, t2) => t1.contains.localeCompare(t2.contains));
            queryClient.setQueryData<Models.TransactionTagRule[]>(["transactionrules"], allTags);
        }
    });
}

export class TransactionTagRuleService extends ServiceBase {

/*    public async addTransactionTag(accountId: string, ruleId: number, tagId: number): Promise<Models.TransactionTagRule> {
        const url = `api/accounts/${accountId}/transaction/tag/rules/${ruleId}/tag/${tagId}`;

        const client = new HttpClient(this.state.app.baseUrl);

        try {
            return await client.put(url);
        }
        catch (response) {
            super.handleError(response as Response);
        }
    }

    public async removeTransactionTag(accountId: string, ruleId: number, tagId: number) {
        const url = `api/accounts/${accountId}/transaction/tag/rules/${ruleId}/tag/${tagId}`;

        const client = new HttpClient(this.state.app.baseUrl);

        try {
            return await client.delete(url);
        }
        catch (response) {
            super.handleError(response as Response);
        }
    }

    public async runRules(accountId: string) {
        const url = `api/accounts/${accountId}/transaction/tag/rules/run`;

        const client = new HttpClient(this.state.app.baseUrl);

        try {
            return await client.post(url, undefined);
        }
        catch (response) {
            super.handleError(response as Response);
        }
    }*/
}