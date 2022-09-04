
export type httpMutatingMethod = "DELETE" | "POST" | "PUT" | "PATCH";
export type httpMethod = "GET" | httpMutatingMethod;

export enum HttpStatusCodes {
    OK = 200,
    Created = 201,
    Accepted = 202,
    NoContent = 204,

    BadRequest = 400,
    Unauthorized = 401,
    Forbidden = 403,
    Conflict = 409,

    InternalServerError = 500,
    ServiceUnavailable = 403,
}

export interface ProblemDetails {
    detail: string,
    instance?: string,
    status?: number,
    title?: string,
    type: string,
}


export default class HttpClient {


    private baseUrl: string;
    private csrf: string;

    constructor(baseUrl: string) {
        this.baseUrl = baseUrl;
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

    postFile<TResponse>(url: string, file: File): Promise<TResponse> {
        return this.fetchWithFormData(url, null, "POST", [file]);
    }

    postFiles<TResponse>(url: string, files: File[]): Promise<TResponse> {
        return this.fetchWithFormData(url, null, "POST", files);
    }

    postFileAndData<TRequest, TResponse>(url: string, file: File, data: TRequest): Promise<TResponse> {
        return this.fetchWithFormData(url, data, "POST", [file]);
    }

    postFilesAndData<TRequest, TResponse>(url: string, files: File[], data: TRequest): Promise<TResponse> {
        return this.fetchWithFormData(url, data, "POST", files);
    }

    private async fetch<T>(url: string, method: httpMethod): Promise<T> {

        const token = await this.getToken();

        const response = await fetch(this.baseUrl + url, {
            credentials: "include",
            headers: new Headers({
                "Accept": "application/json",
                "Authorization": "Bearer " + token,
            }),
            method,
        });

        if (response.status === HttpStatusCodes.NoContent) {
            return null;
        }

        if (response.ok) {
            try {
                return await response.json();
            }
            catch {
                Promise.reject(response);
            }
        }


        return Promise.reject(response);
    }

    private async fetchWithBody<TRequest, TResponse>(url: string, data: TRequest, method: httpMethod): Promise<TResponse> {

        const token = await this.getToken();

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

        if (response.status === HttpStatusCodes.NoContent || response.status === HttpStatusCodes.Accepted) {
            return;
        }

        if (response.ok) {
            try {
                return await response.json();
            }
            catch {
                Promise.reject(response);
            }
        }

        return Promise.reject(response);
    }

    private async fetchWithFormData<TRequest, TResponse>(url: string, data: TRequest, method: httpMethod, files: File[]): Promise<TResponse> {
        const token = await this.getToken();

        const formData = new FormData();

        for (const file of files) {
            formData.append("file", file, file.name);
        }

        const response = await fetch(this.baseUrl + url, {
            body: formData,
            credentials: "include",
            headers: new Headers({
                "Accept": "application/json",
                "Authorization": "Bearer " + token,
            }),
            method,
        });

        if (response.status === HttpStatusCodes.NoContent || response.status === HttpStatusCodes.Accepted) {
            return;
        }

        if (response.ok) {
            try {
                return await response.json();
            }
            catch {
                Promise.reject(response);
            }
        }

        return Promise.reject(response);
    }

    private getToken(): Promise<string> {
        //return Promise.resolve("");
        //return window.getToken(apiRequest, "loginRedirect");
        return Promise.resolve(window.token);
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
