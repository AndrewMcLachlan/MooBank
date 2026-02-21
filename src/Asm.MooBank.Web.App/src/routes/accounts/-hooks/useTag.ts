import { useQuery } from "@tanstack/react-query";
import { getTagOptions } from "api/@tanstack/react-query.gen";

export const useTag = (id: number) => useQuery({
    ...getTagOptions({ path: { id } }),
    enabled: !!id,
});
