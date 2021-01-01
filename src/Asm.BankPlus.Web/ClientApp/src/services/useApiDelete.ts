import { useMutation, MutationOptions } from "react-query";
import { useHttpClient } from "../components";

export const useApiDelete = <Variables>(path: (variables: Variables) => string, options?: MutationOptions<null, null, Variables>) => {

    const httpClient = useHttpClient();

    return useMutation<null, null, Variables>(async (variables): Promise<null> => await httpClient.delete(path(variables)), options);
}