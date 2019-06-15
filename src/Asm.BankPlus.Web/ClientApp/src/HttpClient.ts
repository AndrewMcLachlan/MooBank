import { UserAgentApplication } from "msal";
import * as securityConfiguration from "./securityConfiguration";

export default class HttpClient {

    private baseUrl: string;
    private msal: UserAgentApplication;

    constructor(baseUrl: string, msal: UserAgentApplication) {
        this.baseUrl = baseUrl;
        this.msal = msal;
    }

    public async get<T>(url: string): Promise<T> {
        return this.fetch(url, "GET");
    }

    public put<TRequest, TResponse>(url: string, data: TRequest): Promise<TResponse> {
        return this.fetchWithBody(url, data, "PUT");
    }

    public post<TRequest, TResponse>(url: string, data: TRequest): Promise<TResponse> {
        return this.fetchWithBody(url, data, "POST");
    }

    private async fetch<T>(url: string, method: "GET" | "DELETE"): Promise<T> {

        const token = this.getToken();

        const response = await fetch(this.baseUrl + url, {
            credentials: "same-origin",
            headers: new Headers({
                Accept: "application/json",
                Authorization: "Bearer " + token,
            }),
            method,
        });
        const body = await response.json();

        if (response.ok) {
            return body;
        }

        return Promise.reject(body);
    }

    private async fetchWithBody<TRequest, TResponse>(url: string, data: TRequest, method: "POST" | "PUT" | "PATCH"): Promise<TResponse> {

        const token = this.getToken();

        const response = await fetch(this.baseUrl + url, {
            body: JSON.stringify(data),
            credentials: "same-origin",
            headers: new Headers({
                "Accept": "application/json",
                "Authorization": "Bearer " + token,
                "Content-Type": "application/json",
            }),
            method,
        });

        const body = await response.json();

        if (response.ok === true) {
            return body;
        }

        return Promise.reject(body);
    }

    private async getToken() {

        this.msal.acquireTokenSilent(securityConfiguration.msalRequest).then((tokenResponse) => tokenResponse.accessToken)
            .catch(() => this.msal.acquireTokenPopup(securityConfiguration.msalRequest).then((tokenResponse) => tokenResponse.accessToken)
                .catch((error) => console.log(error)));
    }
}
