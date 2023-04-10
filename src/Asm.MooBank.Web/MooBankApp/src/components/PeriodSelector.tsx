import React, { ChangeEvent, useEffect, useState } from "react";

import format from "date-fns/format";
import parseISO from "date-fns/parseISO";

import { FormGroup, FormRow, FormRow as Row } from "./";
import { Button, Col, Form } from "react-bootstrap";
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

        if (option) {
            setPeriod(option);
        }

        if (!option && props.instant) {
            customPeriodGo();
        }
    }

    const customPeriodGo = () => {
        setPeriod({ startDate: parseISO(customStart), endDate: parseISO(customEnd) });
    }

    const onCustomChange = (setter: React.Dispatch<React.SetStateAction<string>>, value: string) => {
        setter(value);
        if (props.instant) {
            customPeriodGo();
        }
    }

    useEffect(() => {
        props.onChange && props.onChange(period);
        window.localStorage.setItem("report-period", JSON.stringify(period));
    }, [period]);

    return (
        <FormRow>
            <FormGroup xl="4">
                <Form.Label htmlFor="period">Period</Form.Label>
                <Form.Select id="period" onChange={changePeriod} value={selectedPeriod}>
                    {options.map((o, index) =>
                        <option value={o.value} label={o.label} key={index} />
                    )}
                    <option value="0" label="Custom" />
                </Form.Select>
            </FormGroup>
            <FormGroup as={Col} xl={props.instant ? "4" : "3"} hidden={selectedPeriod !== "0"}>
                <Form.Label htmlFor="custom-start">From</Form.Label>
                <Form.Control disabled={selectedPeriod !== "0"} id="custom-start" type="date" value={customStart} onChange={(e) => onCustomChange(setCustomStart, e.currentTarget.value)} />
            </FormGroup>
            <FormGroup xl={props.instant ? "4" : "3"} hidden={selectedPeriod !== "0"}>
                <Form.Label htmlFor="custom-end">To</Form.Label>
                <Form.Control disabled={selectedPeriod !== "0"} id="custom-end" type="date" value={customEnd} onChange={(e) => onCustomChange(setCustomEnd, e.currentTarget.value)} />
            </FormGroup>
            <FormGroup xl="2" className="horizontal-form-controls" hidden={selectedPeriod !== "0" || props.instant}>
                <Button disabled={selectedPeriod !== "0"} onClick={customPeriodGo}>Go</Button>
            </FormGroup>
        </FormRow>
    );
}

PeriodSelector.displayName = "PeriodSelector";

PeriodSelector.defaultProps = {
    instant: false,
};

export interface PeriodSelectorProps {
    value?: Period;
    onChange?: (value: Period) => void;
    instant?: boolean;
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