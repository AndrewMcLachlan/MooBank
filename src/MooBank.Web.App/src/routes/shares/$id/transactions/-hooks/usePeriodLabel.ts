import { useSelector } from "react-redux";

import type { State } from "store/state";
import { getPeriodLabel } from "utils/periodLabel";

export const usePeriodLabel = (): string => {
    const filter = useSelector((s: State) => s.stockTransactions.filter);
    return getPeriodLabel(filter);
};
