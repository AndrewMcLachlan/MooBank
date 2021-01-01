import { useMutation, UseMutationOptions } from "react-query";
import { useHttpClient } from "../components";

export const useApiPatch = <Response, Variables, Data = null>(path: (variables: Variables) => string, options?: UseMutationOptions<Response, null, [Variables, Data]>) => {

    const httpClient = useHttpClient();

    return useMutation<Response, null, [Variables, Data]>(async ([variables, data]): Promise<Response> => await httpClient.patch(path(variables), data), options);
}