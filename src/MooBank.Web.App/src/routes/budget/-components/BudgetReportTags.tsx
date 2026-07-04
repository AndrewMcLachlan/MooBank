import React, { useRef } from "react";

import type { ChartData } from "chart.js";
import { Bar } from "react-chartjs-2";

import { Section } from "@andrewmclachlan/moo-ds";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { overUnderBudgetColours, useChartColours } from "utils/chartColours";
import { useBudgetReportForMonthBreakdown } from "../-hooks/useBudgetReportForMonthBreakdown";
import { useBudgetReportForMonthBreakdownUnbudgeted } from "../-hooks/useBudgetReportForMonthBreakdownUnbudgeted";


export const BudgetReportTags: React.FC<BudgetReportTagsProps> = ({year, month}) => {

    const colours = useChartColours();
    const chartRef = useRef(null);

    const report = useBudgetReportForMonthBreakdown(year, month);
    const reportUnbudgeted = useBudgetReportForMonthBreakdownUnbudgeted(year, month);

    if (!report.data) return null;

    const budgeted = report.data?.tags.map(i => i.budgetedAmount) ?? [];
    const actual = report.data?.tags.map(i => i.actual ?? 0) ?? [];

    const dataset: ChartData<"bar", number[], string> = {
        labels: report.data?.tags.map(t => t.name) ?? [],

        datasets: [{
            label: "Budgeted",
            data: budgeted,
            backgroundColor: colours.neutralTrend,
        }, {
            label: "Actual",
            data: actual,
            backgroundColor: overUnderBudgetColours(actual, budgeted, colours),
        }]
    };

    const datasetUnbudgeted: ChartData<"bar", number[], string> = {
        labels: reportUnbudgeted.data?.tags.map(t => t.name) ?? [],

        datasets: [{
            label: "Actual",
            data: reportUnbudgeted.data?.tags.map(i => i.actual ?? 0),
            backgroundColor: colours.expenses,
        }]
    };

    return (
        <>
            <Section className="report budget-report">
                <h3><FontAwesomeIcon className="clickable" icon="circle-chevron-left" size="xs" onClick={() => window.history.back()} /> Budget Details - {Intl.DateTimeFormat('en', { month: 'long' }).format(new Date(year, month-1))} </h3>
                <div className="budget-report-chart">
                <Bar id="budget-report-tags" ref={chartRef} data={dataset} options={{
                    maintainAspectRatio: false,
                    plugins: {
                        legend: { position: "bottom" },
                        tooltip: {
                            mode: "point",
                            intersect: false,
                        } as any,
                    },
                    scales: {
                        x: { stacked: false, grid: { color: colours.grid } },
                        y: { grid: { color: colours.grid } },
                    }
                }}
                />
                </div>
            </Section>
            <Section className="report budget-report">
                <h3>Unbudgeted Items - {Intl.DateTimeFormat('en', { month: 'long' }).format(new Date(year, month-1))} </h3>
                <div className="budget-report-chart">
                <Bar id="budget-report-unbudgeted" data={datasetUnbudgeted} options={{
                    maintainAspectRatio: false,
                    plugins: {
                        legend: { position: "bottom" },
                        tooltip: {
                            mode: "point",
                            intersect: false,
                        } as any,
                    },
                    scales: {
                        x: { stacked: false, grid: { color: colours.grid } },
                        y: { grid: { color: colours.grid } },
                    }
                }}
                />
                </div>
            </Section>
            </>
    );
}

export interface BudgetReportTagsProps {
    year: number;
    month: number;
}
