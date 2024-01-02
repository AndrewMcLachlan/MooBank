import React, { useEffect, useRef, useState } from "react";

import { Bar } from "react-chartjs-2";
import { Chart as ChartJS, ChartData, registerables } from "chart.js";
import chartTrendline from "chartjs-plugin-trendline";

import { useChartColours } from "helpers/chartColours";
import { Section } from "@andrewmclachlan/mooapp";
import { Col, Form, Row } from "react-bootstrap";
import { useBudgetReport, useBudgetReportForMonthBreakdown, useBudgetYears } from "services/BudgetService";
import { BudgetPage } from "./BudgetPage";
import { UseQueryResult } from "@tanstack/react-query";
import { BudgetReportForMonthBreakdown } from "models";

ChartJS.register(...registerables);
ChartJS.register(chartTrendline);

export const BudgetReportTags: React.FC<BudgetReportTagsProps> = ({year, month}) => {

    const colours = useChartColours();
    const chartRef = useRef();

    const report = useBudgetReportForMonthBreakdown(year, month);

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

    return (
            <Section className="report" style={{ width: "2000px ! important"}}>
                <h3>Budget Report</h3>
                <Bar id="budget-report" ref={chartRef} data={dataset} options={{
                    plugins: {
                        tooltip: {
                            mode: "point",
                            intersect: false,
                        } as any,
                    },
                    hover: {
                        //mode: "point",
                        //intersect: true,
                    },
                    scales: {
                        x: {
                            stacked: false
                        }
                    }
                }}
                   /* onClick={(e) => {
                        var elements = getElementAtEvent(chartRef.current!, e);
                        if (elements.length !== 1) return;
                        if (!report.data!.tags[elements[0].index].hasChildren) return;
                        navigate(`/accounts/${accountId}/reports/breakdown/${report.data!.tags[elements[0].index].tagId}`);
                    }}*/
                />
            </Section>
    );
}

export interface BudgetReportTagsProps {
    year: number;
    month: number;
}