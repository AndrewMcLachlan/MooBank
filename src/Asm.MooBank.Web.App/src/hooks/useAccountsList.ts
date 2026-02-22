import { useQuery } from "@tanstack/react-query";
import { getInstrumentsListOptions } from "api/@tanstack/react-query.gen";

export const useAccountsList = () => useQuery({ ...getInstrumentsListOptions() });
