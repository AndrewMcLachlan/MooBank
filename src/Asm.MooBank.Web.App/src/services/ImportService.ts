import { useApiPostEmpty, useApiPostFile } from "@andrewmclachlan/mooapp";

export const useImportTransactions = () => useApiPostFile<{accountId: string, file: File}>((variables) => `api/accounts/${variables.accountId}/import`);

export const useReprocessTransactions = () => useApiPostEmpty<unknown, {accountId: string}>((variables) => `api/accounts/${variables.accountId}/import/reprocess`);
