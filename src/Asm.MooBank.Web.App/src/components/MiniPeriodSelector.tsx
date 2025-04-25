import React, { ChangeEvent, useEffect, useState } from "react";

import { format } from "date-fns/format";

import { FormGroup, options, PeriodSelectorProps } from ".";
import { Button, Col, Form, Row } from "react-bootstrap";
import { Period, allTime, last12Months, last3Months, last6Months, lastMonth, lastYear, previousMonth, thisMonth, thisYear } from "helpers/dateFns";
import { usePeriod } from "hooks";
import { useLocalStorage } from "@andrewmclachlan/mooapp";

export const MiniPeriodSelector: React.FC<PeriodSelectorProps> = ({ instant = false, cacheKey = "period-id", ...props }) => {

    const [customPeriod, setCustomPeriod] = usePeriod();
    const [selectedPeriod, setSelectedPeriod] = useLocalStorage(cacheKey, "1");
    const [period, setPeriod] = useState<Period>(options.find(o => o.value === selectedPeriod) ?? selectedPeriod === "-1" ? customPeriod : lastMonth);
    const [customStart, setCustomStart] = useState<Date>(customPeriod.startDate);
    const [customEnd, setCustomEnd] = useState<Date>(customPeriod.endDate);

    const changePeriod = (e: ChangeEvent<HTMLSelectElement>) => {
        const index = e.currentTarget.selectedIndex;
        const option = options[index];
        setSelectedPeriod(option?.value ?? "-1");

    }

    useEffect(() => {
        if (selectedPeriod !== "-1") {
            setPeriod(options.find(o => o.value === selectedPeriod) ?? lastMonth);
            return;
        }

        if (selectedPeriod === "-1" && instant) {
            customPeriodGo();
        }
    }, [selectedPeriod]);

    useEffect(() => {
        if (selectedPeriod === "-1") {
            setPeriod(customPeriod);
        }
    }, [customPeriod]);

    const customPeriodGo = () => {
        setCustomPeriod({ startDate: customStart, endDate: customEnd });
    }

    const onCustomStartChange = (value: Date) => {
        setCustomStart(value);

        if (instant) {
            setCustomPeriod({ startDate: value, endDate: customEnd });
        }
    }

    const onCustomEndChange = (value: Date) => {
        setCustomEnd(value);

        if (instant) {
            setCustomPeriod({ startDate: customStart, endDate: value });
        }
    }

    useEffect(() => {
        props.onChange?.(period);
    }, [period]);

    return (
        <>
            <Form.Select id="period" onChange={changePeriod} value={selectedPeriod}>
                {options.map((o, index) =>
                    <option value={o.value} key={index}>{o.label}</option>
                )}
                <option value="-1">Custom</option>
            </Form.Select>
            <Form.Control hidden={selectedPeriod !== "-1"} disabled={selectedPeriod !== "-1"} id="custom-start" type="date" value={customStart ? format(customStart, "yyyy-MM-dd") : ""} onChange={(e) => onCustomStartChange((e.currentTarget as any).valueAsDate)} />
            <Form.Control hidden={selectedPeriod !== "-1"} disabled={selectedPeriod !== "-1"} id="custom-end" type="date" value={customEnd ? format(customEnd, "yyyy-MM-dd") : ""} onChange={(e) => onCustomEndChange((e.currentTarget as any).valueAsDate)} />
            <Button hidden={selectedPeriod !== "-1" || instant} aria-label="Apply custom date filter" disabled={selectedPeriod !== "-1"} onClick={customPeriodGo}>Go</Button>
        </>
    );
}
