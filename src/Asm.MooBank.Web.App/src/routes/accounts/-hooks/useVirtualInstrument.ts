import { useQuery } from "@tanstack/react-query";
import {
    getVirtualInstrumentOptions,
} from "api/@tanstack/react-query.gen";

export const useVirtualInstrument = (accountId: string, virtualInstrumentId: string) => useQuery({
    ...getVirtualInstrumentOptions({ path: { instrumentId: accountId, virtualInstrumentId } }),
});
