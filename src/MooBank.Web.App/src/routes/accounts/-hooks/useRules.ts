import { useQuery } from "@tanstack/react-query";
import {
    getAllInstrumentRulesOptions,
} from "api/@tanstack/react-query.gen";

export const useRules = (accountId: string) => useQuery({
    ...getAllInstrumentRulesOptions({ path: { instrumentId: accountId } }),
    enabled: !!accountId,
});
