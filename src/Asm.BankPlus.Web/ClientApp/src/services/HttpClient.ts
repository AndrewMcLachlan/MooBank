import { UserAgentApplication } from "msal";
import { SecurityService } from "./SecurityService";

export default class HttpClient {

    private baseUrl: string;
    private securityService: SecurityService;
    private csrf: string;

    constructor(baseUrl: string) {
        this.baseUrl = baseUrl;
        this.securityService = new  SecurityService();
        this.csrf = this.readCookie("XSRF-TOKEN");
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

        const token = await this.securityService.getToken();

        const response = await fetch(this.baseUrl + url, {
            credentials: "include",
            headers: new Headers({
                "Accept": "application/json",
                "Authorization": "Bearer " + token,
                "X-XSRF-TOKEN": this.csrf,
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

        const token = await this.securityService.getToken();

        const response = await fetch(this.baseUrl + url, {
            body: JSON.stringify(data),
            credentials: "include",
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

    private readCookie(name: string): string {
        const regex = new RegExp(`${name}=([^;]*)`);
        const match = regex.exec(document.cookie);
        if (match) {
            return match[1];
        }
        return null;
    }
}
