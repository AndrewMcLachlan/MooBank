import { Section } from "@andrewmclachlan/moo-ds";
import { ChartData, Chart as ChartJS, registerables } from "chart.js";
import chartTrendline from "chartjs-plugin-trendline";
import React, { useEffect, useState } from "react";
import { Line } from "react-chartjs-2";
import { Form, Row } from "react-bootstrap";
import { useNavigate, useParams } from "react-router";

import { FormGroup, TagSelector } from "components";
import { PeriodSelector } from "components/PeriodSelector";
import { ReportTypeSelector } from "components/ReportTypeSelector";
import { Period } from "helpers/dateFns";
import { TagSettings } from "models";
import { TrendReportSettings, defaultSettings } from "models/reports";
import { useTag, useTagTrendReport } from "services";
import { useChartColours } from "../../helpers/chartColours";
import { ReportsPage } from "./ReportsPage";
import { TagSettingsPanel } from "./TagSettingsPanel";
import { transactionTypeFilter } from "store/state";
import { MiniPeriodSelector } from "components/MiniPeriodSelector";
import { getPeriod } from "hooks";

ChartJS.register(...registerables);
ChartJS.register(chartTrendline);

export const TagTrend: React.FC = () => {

    const colours = useChartColours();
    const navigate = useNavigate();

    const { id: accountId, tagId } = useParams<{ id: string, tagId: string }>();

    const [reportType, setReportType] = useState<transactionTypeFilter>("Debit");
    const [period, setPeriod] = useState<Period>(getPeriod());
    const [selectedTagId, setSelectedTagId] = useState<number>(tagId ? Number(tagId) : 1);
    const [settings, setSettings] = useState<TrendReportSettings>(defaultSettings);
    const report = useTagTrendReport(accountId!, period?.startDate, period?.endDate, reportType, selectedTagId, settings);

    const { data: selectedTag } = useTag(selectedTagId);

    const dataset: ChartData<"line", number[], string> = {
        labels: report.data?.months.map(i => i.month) ?? [],

        datasets: [{
            label: `${report.data?.tagName} (Net)`,
            data: report.data?.months.map(i => Math.abs(i.netAmount!)) ?? [],
            backgroundColor: colours.income,
            borderColor: colours.income,
            // @ts-expect-error Not a known property for some reason
            trendlineLinear: {
                colorMin: colours.incomeTrend,
                colorMax: colours.incomeTrend,
                lineStyle: "solid",
                width: 2,
            }
        }, {
            label: `${report.data?.tagName} (Gross)`,
            data: report.data?.months.map(i => Math.abs(i.grossAmount)) ?? [],
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
        setSettings({ applySmoothing: settings.applySmoothing, interval: "Monthly" });
    }

    return (
        <ReportsPage title="Tag Trend">
            <Section className="mini-filter-panel">
                <ReportTypeSelector value={reportType} onChange={setReportType} hidden />
                <MiniPeriodSelector onChange={setPeriod} instant />
                <TagSelector value={selectedTagId} onChange={tagChanged} id="filter-tags" />
                <TagSettingsPanel tag={selectedTag} onChange={settingsChanged} />
            </Section>
            <Section className="report" header="Tag Trend" headerSize={3}>
                <Line id="inout" data={dataset} options={{
                    maintainAspectRatio: true,
                    scales: {
                        y: {
                            suggestedMin: 0,
                            ticks: {
                                stepSize: getStepSize(dataset.datasets[0].data),
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
        </ReportsPage >
    );
}

const getStepSize = (values: number[]) => {
    const max = values.reduce((max, d) => d > max ? d : max, -Infinity);
    const magnitude = Math.pow(10, Math.floor(Math.log10(max)));
    const roundedMax = Math.ceil(max / magnitude) * magnitude;
    return roundedMax / 10;
}
