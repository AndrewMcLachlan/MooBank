import { useQuery } from "@tanstack/react-query";
import { getTagsOptions } from "api/@tanstack/react-query.gen";

export const useTags = () => useQuery({
    ...getTagsOptions(),
});
