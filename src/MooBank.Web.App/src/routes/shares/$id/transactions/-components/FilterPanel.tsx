import { Input, Section, Tooltip, useLocalStorage } from "@andrewmclachlan/moo-ds";
import { useEffect, useState } from "react";

import { DateRangeSelector } from "components";

import type { Period } from "models/dateFns";
import { formatISODate } from "utils/dateFns";
import { useStockTransactionSearch } from "../-hooks/useStockTransactionSearch";

export const FilterPanel: React.FC<FilterPanelProps> = (props) => {

    const { setFilter } = useStockTransactionSearch();

    const [filterDescription, setFilterDescription] = useLocalStorage("filter-description", "");
    const [period, setPeriod] = useState<Period>({ startDate: null, endDate: null });

    const clear = () => {
        setFilterDescription("");
    };

    // The query is debounced in useStockTransactionSearch, so typing doesn't fire a request per keystroke.
    useEffect(() => {
        setFilter({ description: filterDescription || undefined, start: period?.startDate && formatISODate(period.startDate), end: period?.endDate && formatISODate(period.endDate) });
    // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [period, filterDescription]);

    return (
        <Section className="mini-filter-panel" {...props}>
            <Tooltip id="filter-desc">Search for multiple terms by separating them with a comma</Tooltip>
            <Input id="filter-desc" type="search" value={filterDescription} onChange={(e) => setFilterDescription(e.currentTarget.value)} placeholder="Description contains..." />
            <DateRangeSelector onChange={setPeriod} />
            <button type="button" className="filter-reset" title="Clear filters" onClick={clear}>Reset</button>
        </Section>
    );
}

export interface FilterPanelProps extends React.HTMLAttributes<HTMLElement> {
}
