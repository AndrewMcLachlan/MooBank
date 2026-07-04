import { useQuery } from "@tanstack/react-query";
import { importerTypesOptions } from "api/@tanstack/react-query.gen";

export const useImporterTypes = () => useQuery({
    ...importerTypesOptions(),
    staleTime: 1000 * 60 * 60 * 24 * 7,
});
