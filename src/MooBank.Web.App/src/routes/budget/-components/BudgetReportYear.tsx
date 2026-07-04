import React from "react";

import type { ChartData } from "chart.js";
import { Bar } from "react-chartjs-2";

import { Section } from "@andrewmclachlan/moo-ds";
import { overUnderBudgetColours, useChartColours } from "utils/chartColours";
import { useBudgetReport } from "../-hooks/useBudgetReport";


export const BudgetReportYear: React.FC<BudgetReportYearProps> = ({ year, onDrilldown }) => {

    const colours = useChartColours();

    const report = useBudgetReport(year);

    const budgeted = report.data?.items.map(i => i.budgetedAmount) ?? [];
    const actual = report.data?.items.map(i => i.actual ?? 0) ?? [];

    const dataset: ChartData<"bar", number[], string> = {
        labels: ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"],

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

    return (
        <Section className="report budget-report">
            <h3>Budget Report</h3>
            <div className="budget-report-chart">
            <Bar id="budget-report" data={dataset} options={{
                maintainAspectRatio: false,
                plugins: {
                    legend: { position: "bottom" },
                    tooltip: {
                        mode: "point",
                        intersect: false,
                    } as any,
                },
                hover: {
                    mode: "point",
                    intersect: true,
                },
                scales: {
                    x: {
                        stacked: false,
                        grid: { color: colours.grid },
                    },
                    y: {
                        grid: { color: colours.grid },
                    },
                },
                onClick: (_event, elements) => {
                    if (elements.length !== 1) return;
                    onDrilldown(report.data!.items[elements[0].index].month);
                },
            }}
            />
            </div>
        </Section>
    );
};

export interface BudgetReportYearProps {
    year: number;
    onDrilldown: (month: number) => void;
}
