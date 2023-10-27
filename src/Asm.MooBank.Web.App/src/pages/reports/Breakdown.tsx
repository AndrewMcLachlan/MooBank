import React, { useEffect, useRef, useState } from "react";

import { ReportsPage } from "./ReportsPage";
import { useAccount, useBreakdownReport, useTag } from "services";

import { Doughnut, getElementAtEvent } from "react-chartjs-2";
import { Chart as ChartJS, ChartData, registerables } from "chart.js";
import { Section, useTheme } from "@andrewmclachlan/mooapp";

import { PeriodSelector } from "components/PeriodSelector";
import { Period } from "helpers/dateFns";
import { ReportType } from "models/reports";
import { ReportTypeSelector } from "components/ReportTypeSelector";
import { useNavigate, useParams } from "react-router-dom";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { chartColours } from "../../helpers/chartColours";

ChartJS.register(...registerables);

export const Breakdown = () => {

    const navigate = useNavigate();

    const { theme, defaultTheme } = useTheme();
    const theTheme = theme ?? defaultTheme;

    const { id: accountId, tagId } = useParams<{ id: string, tagId: string }>();

    const [reportType, setReportType] = useState<ReportType>(ReportType.Expenses);
    const [period, setPeriod] = useState<Period>({ startDate: null, endDate: null });
    const [selectedTagId, setSelectedTagId] = useState<number | undefined>(tagId ? Number(tagId) : undefined);
    const [previousTagId, setPreviousTagId] = useState<number | undefined>();
    const tag = useTag(selectedTagId ?? 0);

    const account = useAccount(accountId!);

    const report = useBreakdownReport(accountId!, period?.startDate, period?.endDate, reportType, selectedTagId);

    const chartRef = useRef();

    useEffect(() => {
        setSelectedTagId(tagId ? parseInt(tagId) : undefined);
    }, [tagId]);

    const dataset: ChartData<"doughnut", number[], string> = {
        labels: report.data?.tags.map(t => t.tagName) ?? [],
        datasets: [{
            label: "",
            data: report.data?.tags.map(t => t.grossAmount) ?? [],
            backgroundColor: chartColours,
        }],
    };

    return (
        <ReportsPage title="Breakdown">
            <Section>
                <ReportTypeSelector value={reportType} onChange={setReportType} />
                <PeriodSelector onChange={setPeriod} />
            </Section>
            <Section className="report doughnut">
                {tag.data?.name && <h3><FontAwesomeIcon className="clickable" icon="circle-chevron-left" size="xs" onClick={() => navigate(-1)} /> {tag.data.name}</h3>}
                {!tag.data && <h3>Top-Level Tags</h3>}
                <Doughnut id="bytag" className="bob" ref={chartRef} data={dataset} options={{

                    plugins: {
                        legend: {
                            position: "right"
                        },
                        tooltip: {
                            mode: "point",
                            intersect: false,
                        } as any,
                    },
                    hover: {
                        mode: "point",
                        intersect: true,
                    },
                }}
                    onClick={(e) => {
                        var elements = getElementAtEvent(chartRef.current!, e);
                        if (elements.length !== 1) return;
                        const tag = report.data!.tags[elements[0].index];
                        if (!tag.hasChildren || tag.tagId === selectedTagId) {

                            const url = !selectedTagId ? `/accounts/${accountId}?untagged=true` : `/accounts/${accountId}?tag=${tag.tagId}`;

                            navigate(url);
                            return;
                        }
                        setPreviousTagId(selectedTagId);
                        setSelectedTagId(tag.tagId);
                        navigate(`/accounts/${accountId}/reports/breakdown/${tag.tagId}`);
                    }} />
            </Section>
        </ReportsPage>
    );
}
