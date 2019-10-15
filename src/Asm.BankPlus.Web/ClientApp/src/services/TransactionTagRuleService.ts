import * as Models from "../models";
import { State, TransactionTagRules } from "../store/state";
import { ServiceBase } from "./ServiceBase";
import HttpClient from "./HttpClient";

export class TransactionTagRuleService extends ServiceBase {

    constructor(state: State) {
        super(state);
    }

    public async getRules(accountId: string): Promise<TransactionTagRules> {
        const url = `api/accounts/${accountId}/transaction/tag/rules`;

        const client = new HttpClient(this.state.app.baseUrl);

        try {
            const rules = await client.get<TransactionTagRules>(url);
            return rules;
        }
        catch (response) {
            super.handleError(response as Response);
        }
    }

    public async createRule(accountId: string, rule: Models.TransactionTagRule): Promise<Models.TransactionTagRule> {
        const url = `api/accounts/${accountId}/transaction/tag/rules`;

        const client = new HttpClient(this.state.app.baseUrl);

        try {
            return await client.put<Models.TransactionTagRule, Models.TransactionTagRule>(url);
        }
        catch (response) {
            await super.handleError(response);
        }
    }

    public async deleteRule(accountId: string, ruleId: number): Promise<void> {
        const url = `api/accounts/${accountId}/transaction/tag/rules/${ruleId}`;

        const client = new HttpClient(this.state.app.baseUrl);

        try {
            return await client.delete<void>(url);
        }
        catch (response) {
            await super.handleError(response);
        }
    }

    public async addTransactionTag(accountId: string, ruleId: number, tagId: number): Promise<Models.TransactionTagRule> {
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
}