import React, { ChangeEvent, ReactEventHandler, SyntheticEvent, useState } from "react";


import format from "date-fns/format";
import getMonth from "date-fns/getMonth";
import getYear from "date-fns/getYear";
import parseISO from "date-fns/parseISO";

import { Page } from "../../layouts";
import { ReportsHeader, TagSelector } from "../../components";
import { useAccount, useInOutReport, useInOutTrendReport, useTagTrendReport } from "../../services";
import { useParams } from "react-router-dom";

import { Line } from "react-chartjs-2";
import { Chart as ChartJS, ChartData, registerables } from "chart.js";
import { useLayout } from "@andrewmclachlan/mooapp";

import { Button, Col, Form, Row } from "react-bootstrap";
import { PeriodSelector } from "../../components/PeriodSelector";
import { useIdParams } from "../../hooks";
import { getCachedPeriod } from "../../helpers";
import { Period } from "../../helpers/dateFns";
import { ReportType } from "../../models/reports";
import { ReportTypeSelector } from "../../components/ReportTypeSelector";

ChartJS.register(...registerables);

export const TagTrend: React.FC = () => {

    const { theme, defaultTheme } = useLayout();

    const theTheme = theme ?? defaultTheme;

    const { id: accountId, tagId } = useParams<{ id: string, tagId: string }>();

    const account = useAccount(accountId!);

    const [reportType, setReportType] = useState<ReportType>(ReportType.Expenses);
    const [period, setPeriod] = useState<Period>(getCachedPeriod());
    const [selectedTagId, setSelectedTagId] = useState<number>(tagId ? parseInt(tagId) : 1);
    const report = useTagTrendReport(accountId!, period.startDate, period.endDate, reportType, selectedTagId);
console.debug(selectedTagId)
    const dataset: ChartData<"line", number[], string> = {
        labels: report.data?.months.map(i => i.month) ?? [],

        datasets: [{
            label: report.data?.tagName,
            data: report.data?.months.map(i => Math.abs(i.amount)) ?? [],
            backgroundColor: theTheme === "dark" ? "#228b22" : "#00FF00",
            borderColor: theTheme === "dark" ? "#228b22" : "#00FF00",
        }]
    };

    return (
        <Page title="Tag Trend">
            <ReportsHeader account={account.data} title="Breakdown" />
            <Page.Content>
                <ReportTypeSelector value={reportType} onChange={setReportType} hidden />
                <PeriodSelector value={period} onChange={setPeriod} />
                <TagSelector value={selectedTagId} onChange={setSelectedTagId} />
                <section className="report">
                    <h3>Tag Trend</h3>
                    <Line id="inout" data={dataset} options={{
                        maintainAspectRatio: true,
                        scales: {
                            y: {
                                suggestedMin: 0,
                                ticks: {
                                    stepSize: 1000,
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
            </Page.Content>
        </Page>
    );
}
