import { useQuery } from "@tanstack/react-query";
import { getMyFamilyOptions } from "api/@tanstack/react-query.gen";

export const useMyFamily = () => useQuery({ ...getMyFamilyOptions() });
