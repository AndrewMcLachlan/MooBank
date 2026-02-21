import { useQuery } from "@tanstack/react-query";
import { getStockHoldingCpiAdjustedGainLossOptions } from "api/@tanstack/react-query.gen";

export const useStockHoldingAdjustedGainLoss = (accountId: string) => useQuery({ ...getStockHoldingCpiAdjustedGainLossOptions({ path: { instrumentId: accountId } }) });
