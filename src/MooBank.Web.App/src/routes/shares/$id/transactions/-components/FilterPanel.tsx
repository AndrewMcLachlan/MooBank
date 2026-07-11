import { Input, Section, Tooltip, useLocalStorage } from "@andrewmclachlan/moo-ds";
import { useEffect, useState } from "react";
import { format } from "date-fns/format";

import { usePeriodSelector } from "components";
import { periodOptions } from "models/periodOptions";

import type { Period } from "models/dateFns";
import { useStockTransactionSearch } from "../-hooks/useStockTransactionSearch";

export const FilterPanel: React.FC<FilterPanelProps> = (props) => {

    const { setFilter } = useStockTransactionSearch();

    const [filterDescription, setFilterDescription] = useLocalStorage("filter-description", "");
    const [period, setPeriod] = useState<Period>({ startDate: null, endDate: null });

    const { changePeriod, selectedPeriod, customStart, onCustomStartChange, customEnd, onCustomEndChange } = usePeriodSelector({ instant: true, cacheKey: "period-id", onChange: setPeriod });

    const clear = () => {
        setFilterDescription("");
    };

    // The query is debounced in useStockTransactionSearch, so typing doesn't fire a request per keystroke.
    useEffect(() => {
        setFilter({ description: filterDescription || undefined, start: period?.startDate?.toISOString(), end: period?.endDate?.toISOString() });
    // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [period, filterDescription]);

    const isCustom = selectedPeriod === "-1";

    return (
        <Section className="mini-filter-panel" {...props}>
            <Tooltip id="filter-desc">Search for multiple terms by separating them with a comma</Tooltip>
            <Input id="filter-desc" type="search" value={filterDescription} onChange={(e) => setFilterDescription(e.currentTarget.value)} placeholder="Description contains..." />
            <Input.Select id="period" onChange={changePeriod} value={selectedPeriod}>
                {periodOptions.map((o, index) =>
                    <option value={o.value} key={index}>{o.label}</option>
                )}
                <option value="-1">Custom</option>
            </Input.Select>
            {isCustom && (
                <>
                    <Input id="custom-start" type="date" value={customStart ? format(customStart, "yyyy-MM-dd") : ""} onChange={(e) => onCustomStartChange((e.currentTarget as any).valueAsDate)} />
                    <Input id="custom-end" type="date" value={customEnd ? format(customEnd, "yyyy-MM-dd") : ""} onChange={(e) => onCustomEndChange((e.currentTarget as any).valueAsDate)} />
                </>
            )}
            <button type="button" className="filter-reset" title="Clear filters" onClick={clear}>Reset</button>
        </Section>
    );
}

export interface FilterPanelProps extends React.HTMLAttributes<HTMLElement> {
}
