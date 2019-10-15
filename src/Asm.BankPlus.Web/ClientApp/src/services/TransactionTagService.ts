import * as Models from "models";
import { State } from "store/state";
import { ServiceBase } from "./ServiceBase";
import HttpClient from "./HttpClient";

export class TransactionTagService extends ServiceBase {
    constructor(state: State) {
        super(state);
    }

    public async getTags() {
        const url = `api/transaction/tags`;

        const client = new HttpClient(this.state.app.baseUrl);

        try {
            return await client.get<Models.TransactionTag[]>(url);
        }
        catch (response) {
            await super.handleError(response);
        }
    }

    public async createTag(name: string): Promise<Models.TransactionTag> {
        const url = `api/transaction/tags/${name}`;

        const client = new HttpClient(this.state.app.baseUrl);

        try {
            return await client.put<undefined, Models.TransactionTag>(url);
        }
        catch (response) {
            await super.handleError(response);
        }
    }

    public async addTransactionTag(tagId: number, subId: number): Promise<Models.Transaction> {
        const url = `api/transaction/tags/${tagId}/tags/${subId}`;

        const client = new HttpClient(this.state.app.baseUrl);

        try {
            return await client.put(url);
        }
        catch (response) {
            super.handleError(response as Response);
        }
    }

    public async removeTransactionTag(tagId: number, subId: number) {
        const url = `api/transaction/tags/${tagId}/tags/${subId}`;

        const client = new HttpClient(this.state.app.baseUrl);

        try {
            return await client.delete(url);
        }
        catch (response) {
            super.handleError(response as Response);
        }
    }
}