import { useQuery } from "@tanstack/react-query";
import { getFormattedInstrumentsListOptions } from "api/@tanstack/react-query.gen";

export const useFormattedAccounts = () => useQuery({
    ...getFormattedInstrumentsListOptions(),
});
