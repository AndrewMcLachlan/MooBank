import React, { ChangeEvent, ReactEventHandler, SyntheticEvent, useState } from "react";
import addMonths from "date-fns/addMonths";
import endOfMonth from "date-fns/endOfMonth";
import startOfMonth from "date-fns/startOfMonth";
import addYears from "date-fns/addYears";
import endOfYear from "date-fns/endOfYear";
import startOfYear from "date-fns/startOfYear";
import format from "date-fns/format";
import { Page } from "../../layouts";
import { ReportsHeader } from "../../components";
import { useAccount, useInOutReport } from "../../services";
import { useParams } from "react-router-dom";

import { Bar } from "react-chartjs-2";
import { Chart as ChartJS, ChartData, registerables } from "chart.js";
import { useLayout } from "@andrewmclachlan/mooapp";
import { getMonth, getYear } from "date-fns";

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

    const [period, setPeriod] = useState([startOfMonth(addMonths(new Date(), -1)), endOfMonth(addMonths(new Date(), -1))]);

    const account = useAccount(accountId!);

    const report = useInOutReport(accountId!, period[0], period[1]);

    const changePeriod = (e: ChangeEvent<HTMLSelectElement>) => {
        const index = e.currentTarget.selectedIndex;
        const option = options[index];

        if (option.value !== "0") {
            setPeriod([option.start!, option.end!]);
        }
        
    }

    var dataset: ChartData<"bar", number[], string> = {
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
                <label>Preiod</label>
                <select className="form-select" onChange={changePeriod}>
                    {options.map((o, index) =>
                        <option value={o.value} label={o.label} key={index} />
                    )}
                </select>
                <section className="inout" >
                    <Bar id="inout" data={dataset} options={{
                        indexAxis: "y",
                        maintainAspectRatio: false
                    }} />
                </section>

                <span>{report.data?.income}</span>
                <span>{report.data?.outgoings}</span>
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

const formatPeriod = (start:Date, end:Date): string => {

    const sameYear = getYear(start) === getYear(end);
    const sameMonth = getMonth(start) === getMonth(end);

    if (sameYear && sameMonth) return format(start, "MMM");

    if (sameYear) return `${format(start, "MMM")} - ${format(end, "MMM")}`;

    return `${format(start, "MMM yy")} - ${format(end, "MMM yy")}`;
}