import { useQuery } from "@tanstack/react-query";
import { getGroupOptions } from "api/@tanstack/react-query.gen";

export const useGroup = (id: string) => useQuery({ ...getGroupOptions({ path: { id } }) });
