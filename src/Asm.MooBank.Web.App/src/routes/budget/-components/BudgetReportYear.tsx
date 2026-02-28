import React, { useRef } from "react";

import { ChartData } from "chart.js";
import { Bar, getElementAtEvent } from "react-chartjs-2";

import { Section } from "@andrewmclachlan/moo-ds";
import { useChartColours } from "utils/chartColours";
import { useBudgetReport } from "../-hooks/useBudgetReport";


export const BudgetReportYear: React.FC<BudgetReportYearProps> = ({ year, onDrilldown }) => {

    const colours = useChartColours();
    const chartRef = useRef(null);

    const report = useBudgetReport(year);

    const dataset: ChartData<"bar", number[], string> = {
        labels: ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"],

        datasets: [{
            label: "Budgeted",
            data: report.data?.items.map(i => i.budgetedAmount) ?? [0, 0, 0, 0, 0, 0, 0, 0, 0, 0],
            backgroundColor: colours.income,
        }, {
            label: "Actual",
            data: report.data?.items.map(i => i.actual ?? 0) ?? [0, 0, 0, 0, 0, 0, 0, 0, 0, 0],
            backgroundColor: colours.expenses,
        }]
    };

    return (
        <Section className="report" style={{ width: "2000px ! important" }}>
            <h3>Budget Report</h3>
            <Bar id="budget-report" ref={chartRef} data={dataset} options={{
                plugins: {
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
                        stacked: false
                    }
                }
            }}
                onClick={(e) => {
                    const elements = getElementAtEvent(chartRef.current!, e);
                    if (elements.length !== 1) return;
                    onDrilldown(report.data!.items[elements[0].index].month);
                }}
            />
        </Section>
    );
};

export interface BudgetReportYearProps {
    year: number;
    onDrilldown: (month: number) => void;
}
