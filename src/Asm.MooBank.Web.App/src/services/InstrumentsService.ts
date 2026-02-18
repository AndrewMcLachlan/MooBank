import { useQuery } from "@tanstack/react-query";
import { getFormattedInstrumentsListOptions, getFormattedInstrumentsListQueryKey, getInstrumentsListOptions } from "api/@tanstack/react-query.gen";
import { AccountList } from "../models";

export const formattedAccountsQueryKey = getFormattedInstrumentsListQueryKey;

// The generated Instrument type is the base class; the API actually returns LogicalAccount objects.
// Cast to AccountList until the full model migration aligns consumer types with generated types.
export const useFormattedAccounts = () => useQuery({
    ...getFormattedInstrumentsListOptions(),
    select: (data) => data as unknown as AccountList,
});

export const useAccountsList = () => useQuery({ ...getInstrumentsListOptions() });
