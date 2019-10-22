import * as Models from "models";
import { State } from "store/state";
import { ServiceBase } from "./ServiceBase";
import HttpClient from "./HttpClient";

export class TransactionTagService extends ServiceBase {
    
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

    public async createTag(name: string, tags?: Models.TransactionTag[]): Promise<Models.TransactionTag> {
        const url = `api/transaction/tags/${name}`;

        const client = new HttpClient(this.state.app.baseUrl);

        try {
            return await client.put<Models.TransactionTag[], Models.TransactionTag>(url, tags);
        }
        catch (response) {
            await super.handleError(response);
        }
    }

    public async deleteTag(tag: Models.TransactionTag) {
        const url = `api/transaction/tags/${tag.id}`;

        const client = new HttpClient(this.state.app.baseUrl);

        try {
            return await client.delete<Models.TransactionTag>(url);
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