import { useQuery } from "@tanstack/react-query";
import { getChargeTypesOptions } from "api/@tanstack/react-query.gen";

export const useChargeTypes = () => useQuery({
    ...getChargeTypesOptions(),
});
