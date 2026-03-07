import { useQuery } from "@tanstack/react-query";
import { getBillAccountSummariesByTypeOptions } from "api/@tanstack/react-query.gen";

export const useBillAccountSummaries = () => useQuery({ ...getBillAccountSummariesByTypeOptions() });
