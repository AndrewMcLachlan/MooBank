import { useQuery, QueryKey } from "react-query";

import { httpClient } from "./HttpClient";

export const useApiQuery = <T>(key: QueryKey, path: string) => useQuery(key, async () : Promise<T> => (await httpClient.get(path)).data);