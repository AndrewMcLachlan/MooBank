import { useQuery } from "@tanstack/react-query";
import { getFormattedInstrumentsListOptions, getFormattedInstrumentsListQueryKey, getInstrumentsListOptions } from "api/@tanstack/react-query.gen";

export const formattedAccountsQueryKey = getFormattedInstrumentsListQueryKey;

export const useFormattedAccounts = () => useQuery({
    ...getFormattedInstrumentsListOptions(),
});

export const useAccountsList = () => useQuery({ ...getInstrumentsListOptions() });
