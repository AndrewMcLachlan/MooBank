import React, { ChangeEvent, ReactEventHandler, SyntheticEvent, useState } from "react";

import addMonths from "date-fns/addMonths";
import endOfMonth from "date-fns/endOfMonth";
import startOfMonth from "date-fns/startOfMonth";
import addYears from "date-fns/addYears";
import endOfYear from "date-fns/endOfYear";
import startOfYear from "date-fns/startOfYear";
import format from "date-fns/format";
import getMonth from "date-fns/getMonth";
import getYear from "date-fns/getYear";
import parseISO from "date-fns/parseISO";

import { Page } from "../../layouts";
import { ReportsHeader } from "../../components";
import { useAccount, useInOutReport } from "../../services";
import { useParams } from "react-router-dom";

import { Bar } from "react-chartjs-2";
import { Chart as ChartJS, ChartData, registerables } from "chart.js";
import { useLayout } from "@andrewmclachlan/mooapp";

import { Button, Col, Form, Row } from "react-bootstrap";

ChartJS.register(...registerables);

export const InOut = () => {

    const { theme, defaultTheme } = useLayout();

    const theTheme = theme ?? defaultTheme;

    const { id: accountId } = useParams<{ id: string }>();

    const options: PeriodOption[] = [
        { value: "1", label: "Last Month", start: startOfMonth(addMonths(new Date(), -1)), end: endOfMonth(addMonths(new Date(), -1)) },
        { value: "2", label: "Previous Month", start: startOfMonth(addMonths(new Date(), -2)), end: endOfMonth(addMonths(new Date(), -2)) },
        { value: "3", label: "Last 3 months", start: startOfMonth(addMonths(new Date(), -3)), end: endOfMonth(addMonths(new Date(), -1)) },
        { value: "4", label: "Last 12 months", start: startOfMonth(addMonths(new Date(), -12)), end: endOfMonth(addMonths(new Date(), -1)) },
        { value: "5", label: "Last year", start: startOfYear(addYears(new Date(), -1)), end: endOfYear(addYears(new Date(), -1)) },
        { value: "0", label: "Custom" },
    ];

    const [selectedPeriod, setSelectedPeriod] = useState<string>("1");
    const [customStart, setCustomStart] = useState<string>(format(startOfMonth(addMonths(new Date(), -1)), "dd/MM/yyy"));
    const [customEnd, setCustomEnd] = useState<string>(format(endOfMonth(addMonths(new Date(), -1)), "dd/MM/yyyy"));
    const [period, setPeriod] = useState([startOfMonth(addMonths(new Date(), -1)), endOfMonth(addMonths(new Date(), -1))]);

    const account = useAccount(accountId!);

    const report = useInOutReport(accountId!, period[0], period[1]);

    const changePeriod = (e: ChangeEvent<HTMLSelectElement>) => {
        const index = e.currentTarget.selectedIndex;
        const option = options[index];
        setSelectedPeriod(option.value);

        if (option.value !== "0") {
            setPeriod([option.start!, option.end!]);
        }

    }

    const customPeriodGo = () => {
        setPeriod([parseISO(customStart), parseISO(customEnd)]);
    }

    const dataset: ChartData<"bar", number[], string> = {
        labels: [formatPeriod(period[0], period[1])],

        datasets: [{
            label: "Income",
            data: [report.data?.income ?? 0],
            backgroundColor: theTheme === "dark" ? "#228b22" : "#00FF00",
            //categoryPercentage: 1
        }, {
            label: "Outgoings",
            data: [Math.abs(report.data?.outgoings ?? 0)],
            backgroundColor: theTheme === "dark" ? "#800020" : "#e23d28",
            //categoryPercentage: 1,
        }]
    };

    return (
        <Page title="Incoming vs Outging">
            <ReportsHeader account={account.data} />
            <Page.Content>
                <Row>
                    <Col xl="4">
                        <Form.Label htmlFor="period">Period</Form.Label>
                        <Form.Select id="period" onChange={changePeriod} value={selectedPeriod}>
                            {options.map((o, index) =>
                                <option value={o.value} label={o.label} key={index} />
                            )}
                        </Form.Select>
                    </Col>
                    <Col xl="3" hidden={selectedPeriod !== "0"}>
                        <Form.Label htmlFor="custom-start">From</Form.Label>
                        <Form.Control id="custom-start" type="date" value={customStart} onChange={(e) => setCustomStart(e.currentTarget.value)} />
                    </Col>
                    <Col xl="3" hidden={selectedPeriod !== "0"}>
                        <Form.Label htmlFor="custom-end">To</Form.Label>
                        <Form.Control id="custom-end" type="date" value={customEnd} onChange={(e) => setCustomEnd(e.currentTarget.value)} />
                    </Col>
                    <Col xl="2" className="horizontal-form-controls" hidden={selectedPeriod !== "0"}>
                        <Button onClick={customPeriodGo}>Go</Button></Col>
                </Row>
                <section className="inout">
                    <Bar id="inout" data={dataset} options={{
                        indexAxis: "y",
                        maintainAspectRatio: false,
                        scales: {
                            y: {
                                grid: {
                                    color: theTheme === "dark" ? "#666" : "#E5E5E5"
                                },
                            },
                            x: {
                                grid: {
                                    color: theTheme === "dark" ? "#666" : "#E5E5E5"
                                },
                            }
                        }
                    }} />
                </section>
            </Page.Content>
        </Page >
    );

}

interface PeriodOption {
    value: string,
    label: string,
    start?: Date,
    end?: Date,
}

const formatPeriod = (start: Date, end: Date): string => {

    const sameYear = getYear(start) === getYear(end);
    const sameMonth = getMonth(start) === getMonth(end);

    if (sameYear && sameMonth) return format(start, "MMM");

    if (sameYear) return `${format(start, "MMM")} - ${format(end, "MMM")}`;

    return `${format(start, "MMM yy")} - ${format(end, "MMM yy")}`;
}