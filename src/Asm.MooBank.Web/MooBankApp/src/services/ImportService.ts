//import { ServiceBase } from "./ServiceBase";
import { useApiPostFile } from "@andrewmclachlan/mooapp";

export const useImportTransactions = () => useApiPostFile<{accountId: string, file: File}>((variables) => `api/accounts/${variables.accountId}/import`);

/*export class ImportService extends ServiceBase {

/*    public async importTransactions(accountId: string, file: File): Promise<void> {
        const url = `api/accounts/${accountId}/import`;

        const client = new HttpClient(this.state.app.baseUrl);

        try {
            await client.postFile(url, file);
        }
        catch (response) {
            super.handleError(response as Response);
        }
    }*/

/*    public async addTransactionTag(transactionId: string, tagId: number): Promise<Models.Transaction> {
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
}*/