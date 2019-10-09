import { UserAgentApplication } from "msal";
import { SecurityService } from "./SecurityService";

export type httpMethod = "GET" | "DELETE" | "POST" | "PUT" | "PATCH";

export enum HttpErrorCodes {
    OK = 200,
    NoContent = 201,
    
    BadRequest = 400,
    Unauthorized = 401,
    Forbidden = 403,
    Conflict = 409,

    InternalServerError = 500,
    ServiceUnavailable = 403,
}

export interface ProblemDetails
{
    detail: string,
    instance?: string,
    status?: number,
    title?: string,
    type: string,
}
 

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

    public async delete<T>(url: string): Promise<T> {
        return this.fetch(url, "DELETE");
    }

    public patch<TRequest, TResponse>(url: string, data: TRequest): Promise<TResponse> {
        return this.fetchWithBody(url, data, "PATCH");
    }

    public put<TRequest, TResponse>(url: string, data?: TRequest): Promise<TResponse> {

        if (!data) {
            return this.fetch(url, "PUT");
        }

        return this.fetchWithBody(url, data, "PUT");
    }

    public post<TRequest, TResponse>(url: string, data: TRequest): Promise<TResponse> {
        return this.fetchWithBody(url, data, "POST");
    }

    private async fetch<T>(url: string, method: httpMethod): Promise<T> {

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

        if (response.status === 201) {
            return null;
        }

        const body = await response.json();

        if (response.ok) {
            return body;
        }

        return Promise.reject(response);
    }

    private async fetchWithBody<TRequest, TResponse>(url: string, data: TRequest, method: httpMethod): Promise<TResponse> {

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
