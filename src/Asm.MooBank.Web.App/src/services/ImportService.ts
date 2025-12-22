import { useApiPostEmpty, useApiPostFile } from "@andrewmclachlan/moo-app";
import { MutationOptions, useQueryClient } from "@tanstack/react-query";
import { accountsKey } from "./AccountService";
import { toast } from "react-toastify";

export const useImportTransactions = () => {

    const client = useQueryClient();

    const { mutateAsync, ...rest } = useApiPostFile<{ instrumentId: string, institutionAccountId: string, file: File }>((variables) => `api/instruments/${variables.instrumentId}/accounts/${variables.institutionAccountId}/import`, {
        onSuccess(data, variables, context) {
            client.invalidateQueries({ queryKey: [accountsKey, variables.instrumentId] });
        },
    });

    const importTransactions = (instrumentId: string, institutionAccountId: string, file: File, options: MutationOptions<null, Error, { instrumentId: string, institutionAccountId: string, file: File }>) => {
        return toast.promise(mutateAsync({ instrumentId, institutionAccountId, file }, options), { pending: "Uploading transactions", success: "Import started", error: "Failed to import transactions" });
    }

    return importTransactions;
}

export const useReprocessTransactions = () => {

    const client = useQueryClient();

    const { mutateAsync, ...rest } = useApiPostEmpty<unknown, { instrumentId: string, institutionAccountId: string }>((variables) => `api/instruments/${variables.instrumentId}/accounts/${variables.institutionAccountId}/import/reprocess`, {
        onSuccess(data, variables, context) {
            client.invalidateQueries({ queryKey: [accountsKey, variables.instrumentId] });
        },
    });

    const reprocessTransactions = (instrumentId: string, institutionAccountId: string, options?: MutationOptions<unknown, Error, { instrumentId: string, institutionAccountId: string }>) => {
        return toast.promise(mutateAsync({ instrumentId, institutionAccountId }, options), { pending: "Requesting reprocessing", success: "Reprocessing started", error: "Failed to reprocess transactions" });
    }

    return reprocessTransactions;
}
