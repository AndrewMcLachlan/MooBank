import * as Models from "../models";
import { ServiceBase } from "./ServiceBase";
import HttpClient from "./HttpClient";

export class ReferenceDataService extends ServiceBase {
    
    public async getImporterTypes() {
        const url = `api/referencedata/importertypes`;

        const client = new HttpClient(this.state.app.baseUrl);

        try {
            return await client.get<Models.ImporterType[]>(url);
        }
        catch (response) {
            await super.handleError(response);
        }
    }

}