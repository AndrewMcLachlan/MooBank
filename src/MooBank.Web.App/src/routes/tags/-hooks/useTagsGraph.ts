import { useQuery } from "@tanstack/react-query";
import { getTagGraphOptions } from "api/@tanstack/react-query.gen";

export const useTagsGraph = () => useQuery({
    ...getTagGraphOptions(),
});
