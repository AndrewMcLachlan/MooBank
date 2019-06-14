export default class HttpClient {

    private baseUrl: string;

    constructor(baseUrl) {
        this.baseUrl = baseUrl;
    }

    public async get<T>(url: string): Promise<T> {
        const response = await fetch(this.baseUrl + url, { credentials: "same-origin" });
        const body = await response.json();

        if (response.ok) {
            return body;
        }

        return Promise.reject(body);
    }

    public put<TRequest, TResponse>(url: string, data: TRequest): Promise<TResponse> {
        return this.postput(url, data, "PUT");
    }

    public post<TRequest, TResponse>(url: string, data: TRequest): Promise<TResponse> {
        return this.postput(url, data, "POST");
    }

    private async postput<TRequest, TResponse>(url: string, data: TRequest, method: "POST" | "PUT"): Promise<TResponse> {
        const response = await fetch(this.baseUrl + url, {
            body: JSON.stringify(data),
            credentials: "same-origin",
            headers: new Headers({
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
}
