import { useEffect, useRef, useState } from "react";

import { useBreakdownReport, useTag } from "services";
import { ReportsPage } from "../ReportsPage";

import { Section } from "@andrewmclachlan/mooapp";
import { ChartData, Chart as ChartJS, registerables } from "chart.js";

import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { PeriodSelector } from "components/PeriodSelector";
import { ReportTypeSelector } from "components/ReportTypeSelector";
import { Period } from "helpers/dateFns";
import { ReportType } from "models/reports";
import { useNavigate, useParams } from "react-router";
import { chartColours } from "../../../helpers/chartColours";
import { Breakdown } from "./Breakdown";

ChartJS.register(...registerables);

export const BreakdownPage = () => {

    const navigate = useNavigate();

    const { id: accountId, tagId } = useParams<{ id: string, tagId: string }>();

    const [reportType, setReportType] = useState<ReportType>(ReportType.Expenses);
    const [period, setPeriod] = useState<Period>({ startDate: null, endDate: null });
    const [selectedTagId, setSelectedTagId] = useState<number | undefined>(tagId ? Number(tagId) : undefined);
    const tag = useTag(selectedTagId ?? 0);

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
              <Breakdown accountId={accountId} tagId={tagId} period={period} reportType={reportType} />
            </Section>
        </ReportsPage>
    );
}
