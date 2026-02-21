import { useQuery } from "@tanstack/react-query";
import { getFamilyOptions } from "api/@tanstack/react-query.gen";

export const useFamily = (id: string) => useQuery({ ...getFamilyOptions({ path: { id } }), enabled: !!id });
