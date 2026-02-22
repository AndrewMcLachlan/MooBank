import { useQuery } from "@tanstack/react-query";
import { getBillAccountOptions } from "api/@tanstack/react-query.gen";

export const useBillAccount = (id: string) => useQuery({
    ...getBillAccountOptions({ path: { instrumentId: id } }),
});
