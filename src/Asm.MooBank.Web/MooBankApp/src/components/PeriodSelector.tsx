import React, { ChangeEvent, useEffect, useState } from "react";

import addMonths from "date-fns/addMonths";
import addYears from "date-fns/addYears";
import endOfMonth from "date-fns/endOfMonth";
import endOfYear from "date-fns/endOfYear";
import format from "date-fns/format";
import parseISO from "date-fns/parseISO";
import startOfMonth from "date-fns/startOfMonth";
import startOfYear from "date-fns/startOfYear";

import { Button, ButtonGroup, Col, Form, Row } from "react-bootstrap";
import { Period, endOfLastMonth, lastMonth, periodEquals, startOfLastMonth } from "../helpers/dateFns";

export const PeriodSelector: React.FC<PeriodSelectorProps> = (props) => {

    const options: PeriodOption[] = [
        { value: "1", label: "Last Month", start: startOfMonth(addMonths(new Date(), -1)), end: endOfMonth(addMonths(new Date(), -1)) },
        { value: "2", label: "Previous Month", start: startOfMonth(addMonths(new Date(), -2)), end: endOfMonth(addMonths(new Date(), -2)) },
        { value: "3", label: "Last 3 months", start: startOfMonth(addMonths(new Date(), -3)), end: endOfMonth(addMonths(new Date(), -1)) },
        { value: "4", label: "Last 12 months", start: startOfMonth(addMonths(new Date(), -12)), end: endOfMonth(addMonths(new Date(), -1)) },
        { value: "5", label: "Last year", start: startOfYear(addYears(new Date(), -1)), end: endOfYear(addYears(new Date(), -1)) },
        { value: "0", label: "Custom" },
    ];

    const [selectedPeriod, setSelectedPeriod] = useState<string>(props.value && !periodEquals(props.value, lastMonth()) ? "0" : "1");
    const [customStart, setCustomStart] = useState<string>(format(props.value?.startDate ?? startOfMonth(addMonths(new Date(), -1)), "yyyy-MM-dd"));
    const [customEnd, setCustomEnd] = useState<string>(format(props.value?.endDate ?? endOfMonth(addMonths(new Date(), -1)), "yyyy-MM-dd"));
    const [period, setPeriod] = useState<Period>(props.value ?? { startDate: startOfLastMonth(), endDate: endOfLastMonth() });

    const changePeriod = (e: ChangeEvent<HTMLSelectElement>) => {
        const index = e.currentTarget.selectedIndex;
        const option = options[index];
        setSelectedPeriod(option.value);

        if (option.value !== "0") {
            setPeriod({ startDate: option.start!, endDate: option.end! });
        }
    }

    const customPeriodGo = () => {
        setPeriod({ startDate: parseISO(customStart), endDate: parseISO(customEnd) });
    }

    useEffect(() => {
        props.onChange && props.onChange(period);
        window.localStorage.setItem("report-period", JSON.stringify(period));
    }, [period]);

    console.debug(customStart);

    return (
        <>
            <Row>
                <Col xl="4">
                    <Form.Label htmlFor="period">Period</Form.Label>
                    <Form.Select id="period" onChange={changePeriod} value={selectedPeriod}>
                        {options.map((o, index) =>
                            <option value={o.value} label={o.label} key={index} />
                        )}
                    </Form.Select>
                </Col>
                <Form.Group as={Col} xl="3" hidden={selectedPeriod !== "0"}>
                    <Form.Label htmlFor="custom-start">From</Form.Label>
                    <Form.Control disabled={selectedPeriod !== "0"} id="custom-start" type="date" value={customStart} onChange={(e) => setCustomStart(e.currentTarget.value)} />
                </Form.Group>
                <Col xl="3" hidden={selectedPeriod !== "0"}>
                    <Form.Label htmlFor="custom-end">To</Form.Label>
                    <Form.Control disabled={selectedPeriod !== "0"} id="custom-end" type="date" value={customEnd} onChange={(e) => setCustomEnd(e.currentTarget.value)} />
                </Col>
                <Col xl="2" className="horizontal-form-controls" hidden={selectedPeriod !== "0"}>
                    <Button disabled={selectedPeriod !== "0"} onClick={customPeriodGo}>Go</Button>
                </Col>
            </Row>
        </>
    )
}

PeriodSelector.displayName = "PeriodSelector";

export interface PeriodSelectorProps {
    value?: Period;
    onChange?: (value: Period) => void;
}

export type PeriodType = "lastMonth" | "previousMonth" | "last3Months" | "last12Months" | "lastYear" | "custom";

interface PeriodOption {
    value: string,
    label: string,
    start?: Date,
    end?: Date,
}