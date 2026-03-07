import { useQuery } from "@tanstack/react-query";
import { getStockHoldingOptions } from "api/@tanstack/react-query.gen";

export const useStockHolding = (accountId: string) => useQuery({
    ...getStockHoldingOptions({ path: { instrumentId: accountId } }),
});
