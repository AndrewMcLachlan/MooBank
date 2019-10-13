import * as Models from "../models";
import { State, Transactions } from "../store/state";
import { ServiceBase } from "./ServiceBase";
import HttpClient from "./HttpClient";

export class ImportService extends ServiceBase {

    constructor(state: State) {
        super(state);
    }

    public async importTransactions(accountId: string, file: File): Promise<void> {
        const url = `api/accounts/${accountId}/import`;

        const client = new HttpClient(this.state.app.baseUrl);

        try {
            await client.postFile(url, file);
        }
        catch (response) {
            super.handleError(response as Response);
        }
    }

    public async addTransactionTag(transactionId: string, tagId: number): Promise<Models.Transaction> {
        const url = `api/transactions/${transactionId}/tag/${tagId}`;

        const client = new HttpClient(this.state.app.baseUrl);

        try {
            return await client.put(url);
        }
        catch (response) {
            super.handleError(response as Response);
        }
    }

    public async removeTransactionTag(transactionId: string, tagId: number) {
        const url = `api/transactions/${transactionId}/tag/${tagId}`;

        const client = new HttpClient(this.state.app.baseUrl);

        try {
            return await client.delete(url);
        }
        catch (response) {
            super.handleError(response as Response);
        }
    }
}