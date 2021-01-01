import { useMutation, UseMutationOptions } from "react-query";
import { useHttpClient } from "../components";

export const useApiPost = <Response, Variables, Data = null>(path: (variables: Variables) => string, options?: UseMutationOptions<Response, null, [Variables, Data]>) => {

    const httpClient = useHttpClient();

    return useMutation<Response, null, [Variables, Data]>(async ([variables, data]): Promise<Response> => await httpClient.post(path(variables), data), options);
}

export const useApiDatalessPost = <Response, Variables>(path: (variables: Variables) => string, options?: UseMutationOptions<Response, null, Variables>) => {

    const httpClient = useHttpClient();

    return useMutation<Response, null, Variables>(async (variables): Promise<Response> => await httpClient.post(path(variables)), options);
}

export const useApiPostFile = <Variables extends { file: File }>(path: (variables: Variables) => string, options?: UseMutationOptions<null, null, Variables>) => {

    const httpClient = useHttpClient();

    return useMutation<null, null, Variables>(async (variables): Promise<null> => {

        const formData = new FormData();

        formData.append("file", variables.file, variables.file.name);

        return (await httpClient.post(path(variables), formData)).data;
    }, options);
}