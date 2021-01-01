import { useQuery, QueryKey, UseQueryOptions, useQueryClient } from "react-query";
import { useHttpClient } from "../components";

export const useApiGet = <T>(key: QueryKey, path: string, options?: UseQueryOptions<T>) => {

    const queryClient = useQueryClient();
    const httpClient = useHttpClient();

    return useQuery<T>(key, async (): Promise<T> => (await httpClient.get<T>(path)).data, {
        onSettled: (data) => {

            let res = queryClient.getQueryCache().find(key);

            if (!res) {
                console.warn(`Query Cache is missing data after get, setting manually: ${path}`);
                queryClient.setQueryData<T>(key, data);
            }
        }
    });
}