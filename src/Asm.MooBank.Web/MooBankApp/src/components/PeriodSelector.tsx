import React, { ChangeEvent, useEffect, useState } from "react";


import format from "date-fns/format";
import parseISO from "date-fns/parseISO";

import { Button, ButtonGroup, Col, Form, Row } from "react-bootstrap";
import { Period, endOfLastMonth, last12Months, last3Months, lastMonth, lastYear, periodEquals, previousMonth, startOfLastMonth } from "../helpers/dateFns";

export const PeriodSelector: React.FC<PeriodSelectorProps> = (props) => {

    const [selectedPeriod, setSelectedPeriod] = useState<string>(getPeriodId(props.value));
    const [customStart, setCustomStart] = useState<string>(format(props.value?.startDate ?? startOfLastMonth(), "yyyy-MM-dd"));
    const [customEnd, setCustomEnd] = useState<string>(format(props.value?.endDate ?? endOfLastMonth(), "yyyy-MM-dd"));
    const [period, setPeriod] = useState<Period>(props.value ?? { startDate: startOfLastMonth(), endDate: endOfLastMonth() });

    useEffect(() => {
        setSelectedPeriod(getPeriodId(props.value));
    }, [props.value]);

    const changePeriod = (e: ChangeEvent<HTMLSelectElement>) => {
        const index = e.currentTarget.selectedIndex;
        const option = options[index];
        setSelectedPeriod(option?.value ?? "0");

        if (option.value !== "0") {
            setPeriod(option);
        }
    }

    const customPeriodGo = () => {
        setPeriod({ startDate: parseISO(customStart), endDate: parseISO(customEnd) });
    }

    useEffect(() => {
        props.onChange && props.onChange(period);
        window.localStorage.setItem("report-period", JSON.stringify(period));
    }, [period]);

    return (
        <>
            <Row>
                <Col xl="4">
                    <Form.Label htmlFor="period">Period</Form.Label>
                    <Form.Select id="period" onChange={changePeriod} value={selectedPeriod}>
                        {options.map((o, index) =>
                            <option value={o.value} label={o.label} key={index} />
                        )}
                        <option value="0" label="Custom" />
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

interface PeriodOption extends Period {
    value: string,
    label: string,
}

const options: PeriodOption[] = [
    { value: "1", label: "Last Month", ...lastMonth },
    { value: "2", label: "Previous Month", ...previousMonth },
    { value: "3", label: "Last 3 months", ...last3Months },
    { value: "4", label: "Last 12 months", ...last12Months },
    { value: "5", label: "Last year", ...lastYear },
    //{ value: "0", label: "Custom" },
];


const getPeriodId = (value?: Period) => {

    if (!value) return "1";

    for (const o of options) {
        if (periodEquals(value, o)) {
            return o.value;
        }
    }

    return "0";
}