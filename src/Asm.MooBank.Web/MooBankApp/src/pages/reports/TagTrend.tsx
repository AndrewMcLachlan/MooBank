import React, { useState } from "react";

import { Page } from "../../layouts";
import { TagSelector } from "../../components";
import { ReportsHeader } from "./ReportsHeader";
import { useAccount, useTagTrendReport } from "../../services";
import { useParams } from "react-router-dom";

import { Line } from "react-chartjs-2";
import { Chart as ChartJS, ChartData, plugins, registerables } from "chart.js";
import { useLayout } from "@andrewmclachlan/mooapp";
import chartTrendline from "chartjs-plugin-trendline";
import { PeriodSelector } from "../../components/PeriodSelector";
import { getCachedPeriod } from "../../helpers";
import { Period } from "../../helpers/dateFns";
import { ReportType } from "../../models/reports";
import { ReportTypeSelector } from "../../components/ReportTypeSelector";

ChartJS.register(...registerables);
ChartJS.register(chartTrendline);

export const TagTrend: React.FC = () => {

    const { theme, defaultTheme } = useLayout();

    const theTheme = theme ?? defaultTheme;

    const { id: accountId, tagId } = useParams<{ id: string, tagId: string }>();

    const account = useAccount(accountId!);

    const [reportType, setReportType] = useState<ReportType>(ReportType.Expenses);
    const [period, setPeriod] = useState<Period>(getCachedPeriod());
    const [selectedTagId, setSelectedTagId] = useState<number>(tagId ? parseInt(tagId) : 1);
    const report = useTagTrendReport(accountId!, period.startDate, period.endDate, reportType, selectedTagId);

    const dataset: ChartData<"line", number[], string> = {
        labels: report.data?.months.map(i => i.month) ?? [],

        datasets: [{
            label: `${report.data?.tagName} (Offset)`,
            data: report.data?.months.map(i => Math.abs(i.offsetAmount!)) ?? [],
            backgroundColor: theTheme === "dark" ? "#228b22" : "#00FF00",
            borderColor: theTheme === "dark" ? "#228b22" : "#00FF00",
            // @ts-ignore
            trendlineLinear: {
                colorMin: theTheme === "dark" ? "#800020" : "#e23d28",
                colorMax: theTheme === "dark" ? "#800020" : "#e23d28",
                lineStyle: "solid",
                width: 2,
            }
        }, {
            label: `${report.data?.tagName} (Expenses)`,
            data: report.data?.months.map(i => Math.abs(i.amount)) ?? [],
            backgroundColor: theTheme === "dark" ? "#800020" : "#e23d28",
            borderColor: theTheme === "dark" ? "#800020" : "#e23d28",

        }]
    };

    return (
        <Page title="Tag Trend">
            <ReportsHeader account={account.data} title="Tag Trend" />
            <Page.Content>
                <ReportTypeSelector value={reportType} onChange={setReportType} hidden />
                <PeriodSelector value={period} onChange={setPeriod} instant />
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
