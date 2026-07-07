import type { Query } from "@tanstack/react-query";
import type { PersistedClient, Persister } from "@tanstack/react-query-persist-client";
import { del, get, set } from "idb-keyval";

// Only stable, non-sensitive reference data is persisted; transactions and
// reports are never written to disk.
const persistedQueries = ["getTags", "getInstrumentsList", "getFormattedInstrumentsList", "importerTypes"];

export const createIDBPersister = (key: IDBValidKey = "moobank-query-cache"): Persister => ({
    persistClient: async (client: PersistedClient) => {
        await set(key, client);
    },
    restoreClient: async () => await get<PersistedClient>(key),
    removeClient: async () => {
        await del(key);
    },
});

export const shouldPersistQuery = (query: Query) =>
    query.state.status === "success" && persistedQueries.includes((query.queryKey[0] as { _id?: string })?._id ?? "");
