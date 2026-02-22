import { getStockHoldingQueryKey } from "api/@tanstack/react-query.gen";

export const stockQueryKey = getStockHoldingQueryKey;

// Preserve old export name for cross-service consumers
export const stockKey = "stock";
