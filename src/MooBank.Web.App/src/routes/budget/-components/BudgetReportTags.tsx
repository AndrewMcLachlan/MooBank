import React, { useRef } from "react";

import { ChartData } from "chart.js";
import { Bar } from "react-chartjs-2";

import { Section } from "@andrewmclachlan/moo-ds";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { useChartColours } from "utils/chartColours";
import { useBudgetReportForMonthBreakdown } from "../-hooks/useBudgetReportForMonthBreakdown";
import { useBudgetReportForMonthBreakdownUnbudgeted } from "../-hooks/useBudgetReportForMonthBreakdownUnbudgeted";


export const BudgetReportTags: React.FC<BudgetReportTagsProps> = ({year, month}) => {

    const colours = useChartColours();
    const chartRef = useRef(null);

    const report = useBudgetReportForMonthBreakdown(year, month);
    const reportUnbudgeted = useBudgetReportForMonthBreakdownUnbudgeted(year, month);

    if (!report.data) return null;

    const dataset: ChartData<"bar", number[], string> = {
        labels: report.data?.tags.map(t => t.name) ?? [],

        datasets: [{
            label: "Budgeted",
            data: report.data?.tags.map(i => i.budgetedAmount),
            backgroundColor: colours.income,
        }, {
            label: "Actual",
            data: report.data?.tags.map(i => i.actual ?? 0),
            backgroundColor: colours.expenses,
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
            <Section className="report" style={{ width: "2000px ! important"}}>
                <h3><FontAwesomeIcon className="clickable" icon="circle-chevron-left" size="xs" onClick={() => window.history.back()} /> Budget Details - {Intl.DateTimeFormat('en', { month: 'long' }).format(new Date(year, month-1))} </h3>
                <Bar id="budget-report" ref={chartRef} data={dataset} options={{
                    plugins: {
                        tooltip: {
                            mode: "point",
                            intersect: false,
                        } as any,
                    },
                    scales: {
                        x: {
                            stacked: false
                        }
                    }
                }}
                />
            </Section>
            <Section className="report" style={{ width: "2000px ! important"}}>
                <h3>Unbudgeted Items - {Intl.DateTimeFormat('en', { month: 'long' }).format(new Date(year, month-1))} </h3>
                <Bar id="budget-report" ref={chartRef} data={datasetUnbudgeted} options={{
                    plugins: {
                        tooltip: {
                            mode: "point",
                            intersect: false,
                        } as any,
                    },
                    scales: {
                        x: {
                            stacked: false
                        }
                    }
                }}
                />
            </Section>
            </>
    );
}

export interface BudgetReportTagsProps {
    year: number;
    month: number;
}
