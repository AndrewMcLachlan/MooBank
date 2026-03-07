import { useQuery } from "@tanstack/react-query";
import { getAssetOptions } from "api/@tanstack/react-query.gen";

export const useAsset = (accountId: string) => useQuery({
    ...getAssetOptions({ path: { id: accountId } }),
});
