import { useQuery, QueryKey, UseQueryOptions } from "react-query";
import { useHttpClient } from "../providers";

export const useApiGet = <T>(key: QueryKey, path: string, options?: UseQueryOptions<T>) => {

    const httpClient = useHttpClient();

    return useQuery<T>(key, async (): Promise<T> => (await httpClient.get<T>(path)).data, options);
}