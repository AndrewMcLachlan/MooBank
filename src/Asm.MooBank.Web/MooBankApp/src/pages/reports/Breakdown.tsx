import React, { useEffect, useRef, useState } from "react";

import format from "date-fns/format";
import getMonth from "date-fns/getMonth";
import getYear from "date-fns/getYear";

import { Page } from "layouts";
import { ReportsHeader } from "./ReportsHeader";
import { useAccount, useBreakdownReport, useTag } from "services";

import { Doughnut, getElementAtEvent } from "react-chartjs-2";
import { Chart as ChartJS, ChartData, registerables } from "chart.js";
import { useLayout } from "@andrewmclachlan/mooapp";

import { useIdParams } from "hooks";
import { PeriodSelector } from "components/PeriodSelector";
import { Period, lastMonth } from "helpers/dateFns";
import { ReportType } from "models/reports";
import { ReportTypeSelector } from "components/ReportTypeSelector";
import { getCachedPeriod } from "helpers";
import { matchRoutes, useLocation, useNavigate, useParams } from "react-router-dom";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { chartColours } from "./chartColours";

ChartJS.register(...registerables);

export const Breakdown = () => {

    const navigate = useNavigate();

    const { theme, defaultTheme } = useLayout();
    const theTheme = theme ?? defaultTheme;

    const { id: accountId, tagId } = useParams<{ id: string, tagId: string }>();

    const [reportType, setReportType] = useState<ReportType>(ReportType.Expenses);
    const [period, setPeriod] = useState<Period>({startDate: null,endDate: null});
    const [selectedTagId, setSelectedTagId] = useState<number | undefined>(tagId ? parseInt(tagId) : undefined);
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
            backgroundColor: chartColours,//theTheme === "dark" ? "#228b22" : "#00FF00",
            //categoryPercentage: 1,
        }],
    };

    return (
        <Page title="Breakdown">
            <ReportsHeader account={account.data} title="Breakdown" />
            <Page.Content>
                <ReportTypeSelector value={reportType} onChange={setReportType} />
                <PeriodSelector onChange={setPeriod} />
                <section className="report doughnut">
                    {tag.data?.name && <h3><FontAwesomeIcon className="clickable" icon="circle-chevron-left" size="xs" onClick={() => navigate(-1)} /> {tag.data.name}</h3>}
                    {!tag.data && <h3>Top-Level Tags</h3>}
                    <Doughnut id="bytag" ref={chartRef} data={dataset} options={{
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
                            if (!report.data!.tags[elements[0].index].hasChildren) return;
                            setPreviousTagId(selectedTagId);
                            setSelectedTagId(report.data!.tags[elements[0].index].tagId);
                            navigate(`/accounts/${accountId}/reports/breakdown/${report.data!.tags[elements[0].index].tagId}`);
                        }} />
                </section>
            </Page.Content>
        </Page >
    );

}

const formatPeriod = (start: Date, end: Date): string => {

    const sameYear = getYear(start) === getYear(end);
    const sameMonth = getMonth(start) === getMonth(end);

    if (sameYear && sameMonth) return format(start, "MMM");

    if (sameYear) return `${format(start, "MMM")} - ${format(end, "MMM")}`;

    return `${format(start, "MMM yy")} - ${format(end, "MMM yy")}`;
}