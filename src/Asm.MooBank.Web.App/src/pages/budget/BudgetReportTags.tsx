import React, { useEffect, useRef, useState } from "react";

import { Bar } from "react-chartjs-2";
import { Chart as ChartJS, ChartData, registerables } from "chart.js";
import chartTrendline from "chartjs-plugin-trendline";

import { useChartColours } from "helpers/chartColours";
import { Section } from "@andrewmclachlan/mooapp";
import { Col, Form, Row } from "react-bootstrap";
import { useBudgetReport, useBudgetReportForMonthBreakdown, useBudgetReportForMonthBreakdownUnbudgeted, useBudgetYears } from "services/BudgetService";
import { BudgetPage } from "./BudgetPage";
import { UseQueryResult } from "@tanstack/react-query";
import { BudgetReportForMonthBreakdown } from "models";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { useNavigate } from "react-router-dom";

ChartJS.register(...registerables);
ChartJS.register(chartTrendline);

export const BudgetReportTags: React.FC<BudgetReportTagsProps> = ({year, month}) => {

    const colours = useChartColours();
    const chartRef = useRef();
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