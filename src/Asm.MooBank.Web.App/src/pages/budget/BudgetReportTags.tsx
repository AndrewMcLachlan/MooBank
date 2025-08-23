import React, { useRef } from "react";

import { ChartData, Chart as ChartJS, registerables } from "chart.js";
import chartTrendline from "chartjs-plugin-trendline";
import { Bar } from "react-chartjs-2";

import { Section } from "@andrewmclachlan/moo-ds";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { useChartColours } from "helpers/chartColours";
import { useNavigate } from "react-router";
import { useBudgetReportForMonthBreakdown, useBudgetReportForMonthBreakdownUnbudgeted } from "services/BudgetService";

ChartJS.register(...registerables);
ChartJS.register(chartTrendline);

export const BudgetReportTags: React.FC<BudgetReportTagsProps> = ({year, month}) => {

    const colours = useChartColours();
    const chartRef = useRef(null);
    const navigate = useNavigate();

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
                <h3><FontAwesomeIcon className="clickable" icon="circle-chevron-left" size="xs" onClick={() => navigate(-1)} /> Budget Details - {Intl.DateTimeFormat('en', { month: 'long' }).format(new Date(year, month-1))} </h3>
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
