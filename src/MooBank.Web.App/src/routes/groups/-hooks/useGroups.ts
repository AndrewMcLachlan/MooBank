import { useQuery } from "@tanstack/react-query";
import { getAllGroupsOptions } from "api/@tanstack/react-query.gen";

export const useGroups = () => useQuery({ ...getAllGroupsOptions() });
