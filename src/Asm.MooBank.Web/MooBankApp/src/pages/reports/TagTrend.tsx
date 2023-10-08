import React, { useEffect, useState } from "react";

import { TagSelector } from "components";
import { ReportsPage } from "./ReportsPage";
import { useAccount, useTag, useTagTrendReport } from "services";
import { useNavigate, useParams } from "react-router-dom";

import { Line } from "react-chartjs-2";
import { Chart as ChartJS, ChartData, plugins, registerables } from "chart.js";
import chartTrendline from "chartjs-plugin-trendline";
import { PeriodSelector } from "components/PeriodSelector";
import { Period } from "helpers/dateFns";
import { ReportType, TagTrendReportSettings, defaultSettings } from "models/reports";
import { ReportTypeSelector } from "components/ReportTypeSelector";
import { TagSettingsPanel } from "./TagSettingsPanel";
import { TagSettings } from "models";
import { useChartColours } from "../../helpers/chartColours";
import { Section } from "@andrewmclachlan/mooapp";

ChartJS.register(...registerables);
ChartJS.register(chartTrendline);

export const TagTrend: React.FC = () => {

    const colours = useChartColours();
    const navigate = useNavigate();

    const { id: accountId, tagId } = useParams<{ id: string, tagId: string }>();

    const account = useAccount(accountId!);

    const [reportType, setReportType] = useState<ReportType>(ReportType.Expenses);
    const [period, setPeriod] = useState<Period>({ startDate: null, endDate: null });
    const [selectedTagId, setSelectedTagId] = useState<number>(tagId ? Number(tagId) : 1);
    const [settings, setSettings] = useState<TagTrendReportSettings>(defaultSettings);
    const report = useTagTrendReport(accountId!, period?.startDate, period?.endDate, reportType, selectedTagId, settings);

    const { data: selectedTag } = useTag(selectedTagId);

    const dataset: ChartData<"line", number[], string> = {
        labels: report.data?.months.map(i => i.month) ?? [],

        datasets: [{
            label: `${report.data?.tagName} (Offset)`,
            data: report.data?.months.map(i => Math.abs(i.offsetAmount!)) ?? [],
            backgroundColor: colours.income,
            borderColor: colours.income,
            // @ts-ignore
            trendlineLinear: {
                colorMin: colours.incomeTrend,
                colorMax: colours.incomeTrend,
                lineStyle: "solid",
                width: 2,
            }
        }, {
            label: `${report.data?.tagName} (Expenses)`,
            data: report.data?.months.map(i => Math.abs(i.amount)) ?? [],
            backgroundColor: colours.expenses,
            borderColor: colours.expenses,

        }]
    };

    useEffect(() => {
        setSelectedTagId(tagId ? Number(tagId) : 1);
    }, [tagId]);

    const tagChanged = (id: number | number[]) => {
        setSelectedTagId(Number(id));
        navigate(`/accounts/${accountId}/reports/tag-trend/${id}`);
    }

    const settingsChanged = (settings: TagSettings) => {
        setSettings({ applySmoothing: settings.applySmoothing });
    }

    return (
        <ReportsPage title="Tag Trend">
            <Section>
                <ReportTypeSelector value={reportType} onChange={setReportType} hidden />
                <PeriodSelector onChange={setPeriod} instant />
                <TagSettingsPanel tag={selectedTag} onChange={settingsChanged} />
                <TagSelector value={selectedTagId} onChange={tagChanged} />
            </Section>
            <Section className="report" title="Tag Trend" size={3}>
                <Line id="inout" data={dataset} options={{
                    maintainAspectRatio: true,
                    scales: {
                        y: {
                            suggestedMin: 0,
                            ticks: {
                                stepSize: 1000,
                            },
                            grid: {
                                color: colours.grid,
                            },
                        },
                        x: {
                            grid: {
                                color: colours.grid,
                            },
                        }
                    }
                }} />
            </Section>
            <Section className="averages">
                <p>Average: {report.data?.average}</p>
                <p>Average (Offset): {report.data?.offsetAverage}</p>
            </Section>
        </ReportsPage>
    );
}
