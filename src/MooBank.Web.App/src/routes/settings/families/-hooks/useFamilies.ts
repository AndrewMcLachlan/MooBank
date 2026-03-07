import { useQuery } from "@tanstack/react-query";
import { getAllFamiliesOptions } from "api/@tanstack/react-query.gen";

export const useFamilies = () => useQuery({ ...getAllFamiliesOptions(), staleTime: 1000 * 60 * 5 });
