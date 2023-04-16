import React, { useEffect, useState } from "react";

import { Page } from "layouts";
import { TagSelector } from "components";
import { ReportsHeader } from "./ReportsHeader";
import { useAccount, useTag, useTagTrendReport } from "services";
import { useNavigate, useParams } from "react-router-dom";

import { Line } from "react-chartjs-2";
import { Chart as ChartJS, ChartData, plugins, registerables } from "chart.js";
import { useLayout } from "@andrewmclachlan/mooapp";
import chartTrendline from "chartjs-plugin-trendline";
import { PeriodSelector } from "components/PeriodSelector";
import { getCachedPeriod } from "helpers";
import { Period } from "helpers/dateFns";
import { ReportType, TagTrendReportSettings, defaultSettings } from "models/reports";
import { ReportTypeSelector } from "components/ReportTypeSelector";
import { TagSettingsPanel } from "./TagSettingsPanel";
import { TransactionTagSettings } from "models";

ChartJS.register(...registerables);
ChartJS.register(chartTrendline);

export const TagTrend: React.FC = () => {

    const { theme } = useLayout();
    //const theTheme = theme ?? defaultTheme;
    const navigate = useNavigate();

    const { id: accountId, tagId } = useParams<{ id: string, tagId: string }>();

    const account = useAccount(accountId!);

    const [reportType, setReportType] = useState<ReportType>(ReportType.Expenses);
    const [period, setPeriod] = useState<Period>({startDate: null,endDate: null});
    const [selectedTagId, setSelectedTagId] = useState<number>(tagId ? Number(tagId) : 1);
    const [settings, setSettings] = useState<TagTrendReportSettings>(defaultSettings);
    const report = useTagTrendReport(accountId!, period?.startDate, period?.endDate, reportType, selectedTagId, settings);

    const { data: selectedTag } = useTag(selectedTagId);

    const dataset: ChartData<"line", number[], string> = {
        labels: report.data?.months.map(i => i.month) ?? [],

        datasets: [{
            label: `${report.data?.tagName} (Offset)`,
            data: report.data?.months.map(i => Math.abs(i.offsetAmount!)) ?? [],
            backgroundColor: theme === "dark" ? "#228b22" : "#00FF00",
            borderColor: theme === "dark" ? "#228b22" : "#00FF00",
            // @ts-ignore
            trendlineLinear: {
                colorMin: theme === "dark" ? "#800020" : "#e23d28",
                colorMax: theme === "dark" ? "#800020" : "#e23d28",
                lineStyle: "solid",
                width: 2,
            }
        }, {
            label: `${report.data?.tagName} (Expenses)`,
            data: report.data?.months.map(i => Math.abs(i.amount)) ?? [],
            backgroundColor: theme === "dark" ? "#800020" : "#e23d28",
            borderColor: theme === "dark" ? "#800020" : "#e23d28",

        }]
    };

    useEffect(() => {
        setSelectedTagId(tagId ? Number(tagId) : 1);
    }, [tagId]);

    const tagChanged = (id: number) => {
        setSelectedTagId(id);
        navigate(`/accounts/${accountId}/reports/tag-trend/${id}`);
    }

    const settingsChanged = (settings: TransactionTagSettings) => {
        setSettings({applySmoothing: settings.applySmoothing});
    }

    return (
        <Page title="Tag Trend">
            <ReportsHeader account={account.data} title="Tag Trend" />
            <Page.Content>
                <ReportTypeSelector value={reportType} onChange={setReportType} hidden />
                <PeriodSelector onChange={setPeriod} instant />
                <TagSettingsPanel tag={selectedTag} onChange={settingsChanged} />
                <TagSelector value={selectedTagId} onChange={tagChanged} />
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
                                    color: theme === "dark" ? "#333" : "#E5E5E5"
                                },
                            },
                            x: {
                                grid: {
                                    color: theme === "dark" ? "#333" : "#E5E5E5"
                                },
                            }
                        }
                    }} />
                </section>
                <section className="averages">
                    <p>Average: {report.data?.average}</p>
                    <p>Average (Offset): {report.data?.offsetAverage}</p>
                </section>
            </Page.Content>
        </Page>
    );
}
