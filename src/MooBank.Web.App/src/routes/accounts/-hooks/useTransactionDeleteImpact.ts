import { useQuery } from "@tanstack/react-query";
import { getTransactionDeleteImpactOptions } from "api/@tanstack/react-query.gen";

/**
 * What deleting a transaction would take with it. Only fetched while `enabled`, so the confirmation
 * dialog pays for it and the transaction list does not.
 */
export const useTransactionDeleteImpact = (instrumentId: string, transactionId: string, enabled: boolean) =>
    useQuery({
        ...getTransactionDeleteImpactOptions({ path: { instrumentId, id: transactionId } }),
        enabled,
    });
