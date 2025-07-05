import React, { ChangeEvent, useEffect, useState } from "react";
import { useDispatch } from "react-redux";

import { format } from "date-fns/format";

import { FormGroup  } from "./";
import { Button, Col, Form, Row } from "react-bootstrap";
import { Period, lastMonth } from "helpers/dateFns";

import { useCustomPeriod } from "hooks";
import { useLocalStorage } from "@andrewmclachlan/mooapp";
import { periodOptions } from "models";


export const PeriodSelector: React.FC<PeriodSelectorProps> = ({instant = false, cacheKey = "period-id", ...props}) => {

    const {changePeriod, selectedPeriod, customStart, onCustomStartChange, customEnd, onCustomEndChange, customPeriodGo} = usePeriodSelector({instant, cacheKey, ...props});

    return (
        <Row as="section">
            <FormGroup xl="4">
                <Form.Label htmlFor="period">Period</Form.Label>
                <Form.Select id="period" onChange={changePeriod} value={selectedPeriod}>
                    {periodOptions.map((o, index) =>
                        <option value={o.value} key={index}>{o.label}</option>
                    )}
                    <option value="-1">Custom</option>
                </Form.Select>
            </FormGroup>
            <FormGroup as={Col} xl={instant ? "4" : "3"} hidden={selectedPeriod !== "-1"}>
                <Form.Label htmlFor="custom-start">From</Form.Label>
                <Form.Control disabled={selectedPeriod !== "-1"} id="custom-start" type="date" value={customStart ? format(customStart, "yyyy-MM-dd") :""} onChange={(e) => onCustomStartChange((e.currentTarget as any).valueAsDate)} />
            </FormGroup>
            <FormGroup xl={instant ? "4" : "3"} hidden={selectedPeriod !== "-1"}>
                <Form.Label htmlFor="custom-end">To</Form.Label>
                <Form.Control disabled={selectedPeriod !== "-1"} id="custom-end" type="date" value={customEnd ? format(customEnd, "yyyy-MM-dd"): ""} onChange={(e) => onCustomEndChange((e.currentTarget as any).valueAsDate)} />
            </FormGroup>
            <FormGroup xl="2" className="horizontal-form-controls" hidden={selectedPeriod !== "-1" || instant}>
                <Button aria-label="Apply custom date filter" disabled={selectedPeriod !== "-1"} onClick={customPeriodGo}>Go</Button>
            </FormGroup>
        </Row>
    );
}

export interface PeriodSelectorProps {
    value?: Period;
    onChange?: (value: Period) => void;
    instant?: boolean;
    cacheKey?: string;
}

export type PeriodType = "thisMonth" | "lastMonth" | "previousMonth" | "last3Months" | "last6Months" | "last12Months" | "lastYear" | "allTime" | "custom";


export const usePeriodSelector = ({instant, cacheKey, ...props}: PeriodSelectorProps) => {
    const [customPeriod, setCustomPeriod] = useCustomPeriod();
    const [selectedPeriod, setSelectedPeriod] = useLocalStorage(cacheKey, "1");
    const [period, setPeriod] = useState<Period>(periodOptions.find(o => o.value === selectedPeriod) ?? (selectedPeriod === "-1" ? customPeriod : lastMonth));
    const [customStart, setCustomStart] = useState<Date>(customPeriod.startDate);
    const [customEnd, setCustomEnd] = useState<Date>(customPeriod.endDate);

    const changePeriod = (e: ChangeEvent<HTMLSelectElement>) => {
        const index = e.currentTarget.selectedIndex;
        const option = periodOptions[index];
        setSelectedPeriod(option?.value ?? "-1");
    }

    useEffect(() => {
        if (selectedPeriod !== "-1") {
            setPeriod(periodOptions.find(o => o.value === selectedPeriod) ?? lastMonth);
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

    return {
        period,
        setPeriod,
        selectedPeriod,
        setSelectedPeriod,
        customStart,
        setCustomStart,
        customEnd,
        setCustomEnd,
        changePeriod,
        onCustomStartChange,
        onCustomEndChange,
        customPeriodGo
    };
}
