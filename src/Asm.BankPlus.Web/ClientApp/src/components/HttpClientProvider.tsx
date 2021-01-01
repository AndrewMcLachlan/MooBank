import React, { useContext } from "react";
import axios, { AxiosInstance } from "axios";

import { apiRequest, useMsal } from ".";

export interface HttpClientProviderProps {
    baseUrl: string;
}

export const HttpClientContext = React.createContext<AxiosInstance | undefined>(undefined);
export const HttpClientProvider: React.FC<React.PropsWithChildren<HttpClientProviderProps>> = ({ baseUrl, children }) => {

    return (
        <HttpClientContext.Provider value={useCreateHttpClient(baseUrl)}>
            {children}
        </HttpClientContext.Provider>
    );
};

export const useHttpClient = () => useContext(HttpClientContext);

const useCreateHttpClient = (baseUrl: string): AxiosInstance => {

    const {  getToken } = useMsal();

    const httpClient = axios.create({
        baseURL: baseUrl,
        headers: {
            "Accept": "application/json",
        }
    });

    httpClient.interceptors.request.use(async (request) => {
        request.headers = {
            "Authorization": `Bearer ${await getToken(apiRequest, "loginRedirect")}`
        };
        return request;
    });

    return httpClient;
}