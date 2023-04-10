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
import { PeriodSelector } from "../../components/PeriodSelector";
import { useIdParams } from "../../hooks";
import { getCachedPeriod } from "../../helpers";
import { InOutTrend } from "./InOutTrend";

ChartJS.register(...registerables);

export const InOut = () => {

    const { theme, defaultTheme } = useLayout();

    const theTheme = theme ?? defaultTheme;

    const accountId= useIdParams();

    const [period, setPeriod] = useState(getCachedPeriod());

    const account = useAccount(accountId!);


    const report = useInOutReport(accountId!, period.startDate, period.endDate);

    const dataset: ChartData<"bar", number[], string> = {
        labels: [formatPeriod(period.startDate, period.endDate)],

        datasets: [{
            label: "Income",
            data: [report.data?.income ?? 0],
            backgroundColor: theTheme === "dark" ? "#228b22" : "#00FF00",
            //categoryPercentage: 1
        }, {
            label: "Exprenses",
            data: [Math.abs(report.data?.outgoings ?? 0)],
            backgroundColor: theTheme === "dark" ? "#800020" : "#e23d28",
            //categoryPercentage: 1,
        }]
    };

    return (
        <Page title="Incoming vs Outging">
            <ReportsHeader account={account.data} title="Income vs Expenses" />
            <Page.Content>
             <PeriodSelector value={period} onChange={setPeriod} />
                <section className="report inout">
                    <h3>Total Income vs Expenses</h3>
                    <Bar id="inout" data={dataset} options={{
                        indexAxis: "y",
                        maintainAspectRatio: false,
                        scales: {
                            y: {
                                grid: {
                                    color: theTheme === "dark" ? "#333" : "#E5E5E5"
                                },
                            },
                            x: {
                                grid: {
                                    color: theTheme === "dark" ? "#333" : "#E5E5E5"
                                },
                            }
                        }
                    }} />
                </section>
                <InOutTrend period={period} />
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