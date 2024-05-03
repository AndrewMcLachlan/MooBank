import { useApiPostEmpty, useApiPostFile } from "@andrewmclachlan/mooapp";

export const useImportTransactions = () => useApiPostFile<{instrumentId: string, file: File}>((variables) => `api/instruments/${variables.instrumentId}/import`);

export const useReprocessTransactions = () => useApiPostEmpty<unknown, {instrumentId: string}>((variables) => `api/instruments/${variables.instrumentId}/import/reprocess`);
