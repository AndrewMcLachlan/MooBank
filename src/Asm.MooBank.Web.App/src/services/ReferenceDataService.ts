import { useQuery } from "@tanstack/react-query";
import { importerTypesOptions } from "api/@tanstack/react-query.gen";

export const useImporterTypes = () => useQuery({ ...importerTypesOptions() });
