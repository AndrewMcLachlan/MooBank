import { useQuery } from "@tanstack/react-query";
import {
    getVirtualInstrumentsOptions,
} from "api/@tanstack/react-query.gen";

export const useVirtualInstruments = (accountId: string) => useQuery({
    ...getVirtualInstrumentsOptions({ path: { instrumentId: accountId } }),
});
