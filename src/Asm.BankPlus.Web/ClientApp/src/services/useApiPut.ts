import { useMutation, UseMutationOptions } from "react-query";
import { useHttpClient } from "../providers";

export const useApiPut = <Response, Variables, Data = null>(path: (variables: Variables) => string, options?: UseMutationOptions<Response, null, [Variables, Data]>) => {

    const httpClient = useHttpClient();

    return useMutation<Response, null,[Variables, Data]>(async ([variables, data]): Promise<Response> => await httpClient.put(path(variables), data), options);
}

export const useApiDatalessPut = <Response, Variables>(path: (variables: Variables) => string, options?: UseMutationOptions<Response, null, Variables>) => {

    const httpClient = useHttpClient();

    return useMutation<Response, null,Variables>(async (variables): Promise<Response> => await httpClient.put(path(variables)), options);
}