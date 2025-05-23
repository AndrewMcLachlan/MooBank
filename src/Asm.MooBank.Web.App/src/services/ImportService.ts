import { useApiPostEmpty, useApiPostFile } from "@andrewmclachlan/mooapp";
import { MutationOptions, useQueryClient } from "@tanstack/react-query";
import { accountsKey } from "./AccountService";
import { toast } from "react-toastify";

export const useImportTransactions = () => {

    const client = useQueryClient();

    const { mutateAsync, ...rest } = useApiPostFile<{ instrumentId: string, file: File }>((variables) => `api/instruments/${variables.instrumentId}/import`, {
        onSuccess(data, variables, context) {
            client.invalidateQueries({ queryKey: [accountsKey, variables.instrumentId] });
        },
    });

    const importTransactions = (instrumentId: string, file: File, options: MutationOptions<null, Error, { instrumentId: string, file: File }>) => {
        return toast.promise(mutateAsync({ instrumentId, file }, options), { pending: "Importing transactions", success: "Transactions imported", error: "Failed to import transactions" });
    }

    return importTransactions;
}

export const useReprocessTransactions = () => {

    const { mutateAsync, ...rest } = useApiPostEmpty<unknown, { instrumentId: string }>((variables) => `api/instruments/${variables.instrumentId}/import/reprocess`);

    const reprocessTransactions = (instrumentId: string) => {
        return toast.promise(mutateAsync({ instrumentId }), { pending: "Requesting reprocessing", success: "Reprocessing started", error: "Failed to reprocess transactions" });
    }

    return reprocessTransactions;
}
