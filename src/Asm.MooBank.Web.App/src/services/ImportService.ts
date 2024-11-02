import { useApiPostEmpty, useApiPostFile } from "@andrewmclachlan/mooapp";
import { useQueryClient } from "@tanstack/react-query";
import { accountsKey } from "./AccountService";

export const useImportTransactions = () => {

    const client = useQueryClient();

    return useApiPostFile<{ instrumentId: string, file: File }>((variables) => `api/instruments/${variables.instrumentId}/import`, {
        onSuccess(data, variables, context) {
            client.invalidateQueries({ queryKey: [accountsKey, variables.instrumentId] });
        },
    });
}

export const useReprocessTransactions = () => useApiPostEmpty<unknown, { instrumentId: string }>((variables) => `api/instruments/${variables.instrumentId}/import/reprocess`);
