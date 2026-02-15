import { format } from "date-fns/format";

import { PeriodSelectorProps, usePeriodSelector } from ".";
import { Button, Form, Input } from "@andrewmclachlan/moo-ds";
import { periodOptions } from "models";

export const MiniPeriodSelector: React.FC<PeriodSelectorProps> = ({ instant = false, cacheKey = "period-id", ...props }) => {

    const {changePeriod, selectedPeriod, customStart, onCustomStartChange, customEnd, onCustomEndChange, customPeriodGo} = usePeriodSelector({instant, cacheKey, ...props});

    return (
        <>
            <Input.Select id="period" onChange={changePeriod} value={selectedPeriod}>
                {periodOptions.map((o, index) =>
                    <option value={o.value} key={index}>{o.label}</option>
                )}
                <option value="-1">Custom</option>
            </Input.Select>
            <Input hidden={selectedPeriod !== "-1"} disabled={selectedPeriod !== "-1"} id="custom-start" type="date" value={customStart ? format(customStart, "yyyy-MM-dd") : ""} onChange={(e) => onCustomStartChange((e.currentTarget as any).valueAsDate)} />
            <Input hidden={selectedPeriod !== "-1"} disabled={selectedPeriod !== "-1"} id="custom-end" type="date" value={customEnd ? format(customEnd, "yyyy-MM-dd") : ""} onChange={(e) => onCustomEndChange((e.currentTarget as any).valueAsDate)} />
            <Button hidden={selectedPeriod !== "-1" || instant} aria-label="Apply custom date filter" disabled={selectedPeriod !== "-1"} onClick={customPeriodGo}>Go</Button>
        </>
    );
}
