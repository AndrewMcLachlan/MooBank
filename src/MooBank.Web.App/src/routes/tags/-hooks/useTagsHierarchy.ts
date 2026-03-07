import { useQuery } from "@tanstack/react-query";
import { getTagHierarchyOptions } from "api/@tanstack/react-query.gen";

export const useTagsHierarchy = () => useQuery({
    ...getTagHierarchyOptions(),
});
