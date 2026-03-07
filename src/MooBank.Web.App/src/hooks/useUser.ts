import { useQuery } from "@tanstack/react-query";
import { getUserOptions } from "api/@tanstack/react-query.gen";

export const useUser = () => useQuery({ ...getUserOptions() });
