import React, { ChangeEvent, ReactEventHandler, SyntheticEvent, useState } from "react";


import format from "date-fns/format";
import getMonth from "date-fns/getMonth";
import getYear from "date-fns/getYear";
import parseISO from "date-fns/parseISO";

import { Page } from "../../layouts";
import { ReportsHeader } from "../../components";
import { useAccount, useInOutReport, useInOutTrendReport } from "../../services";
import { useParams } from "react-router-dom";

import { Line } from "react-chartjs-2";
import { Chart as ChartJS, ChartData, registerables } from "chart.js";
import { useLayout } from "@andrewmclachlan/mooapp";

import { Button, Col, Form, Row } from "react-bootstrap";
import { PeriodSelector } from "../../components/PeriodSelector";
import { useIdParams } from "../../hooks";
import { getCachedPeriod } from "../../helpers";
import { Period } from "../../helpers/dateFns";

ChartJS.register(...registerables);

export const InOutTrend: React.FC<InOutTrendProps> = ({period}) => {

    const { theme, defaultTheme } = useLayout();

    const theTheme = theme ?? defaultTheme;

    const accountId = useIdParams();

    const report = useInOutTrendReport(accountId!, period.startDate, period.endDate);

    const dataset: ChartData<"line", number[], string> = {
        labels: report.data?.income.map(i => i.month) ?? [],

        datasets: [{
            label: "Income",
            data: report.data?.income.map(i => i.amount) ?? [],
            backgroundColor: theTheme === "dark" ? "#228b22" : "#00FF00",
            borderColor: theTheme === "dark" ? "#228b22" : "#00FF00",
            showLine: true,
            //categoryPercentage: 1
        }, {
            label: "Expenses",
            data: report.data?.expenses.map(i => Math.abs(i.amount)) ?? [],
            backgroundColor: theTheme === "dark" ? "#800020" : "#e23d28",
            borderColor: theTheme === "dark" ? "#800020" : "#e23d28",
            //categoryPercentage: 1,
        }]
    };

    return (
        <section className="report">
            <h3>Income vs Expenses per Month</h3>
            <Line id="inout" data={dataset} options={{
                maintainAspectRatio: true,
                scales: {
                    y: {
                        suggestedMin: 0,
                        ticks: {
                            stepSize: 5000,
                        },
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
    );
}

export interface InOutTrendProps {
    period: Period;
}