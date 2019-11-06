import * as Models from "models";
import { Transactions } from "store/state";
import { ServiceBase } from "./ServiceBase";
import HttpClient from "./HttpClient";

export class TransactionService extends ServiceBase {

    public async getTransactions(accountId: string, pageSize: number, pageNumber: number): Promise<Transactions> {
        const url = `api/accounts/${accountId}/transactions/${pageSize}/${pageNumber}`;

        const client = new HttpClient(this.state.app.baseUrl);

        try {
            const transactions = await client.get<Transactions>(url);
            transactions.currentPage = pageNumber;
            return transactions;
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