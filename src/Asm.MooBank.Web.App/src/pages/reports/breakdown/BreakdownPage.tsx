import { useEffect, useRef, useState } from "react";

import { useTag } from "services";
import { ReportsPage } from "../ReportsPage";

import { Section } from "@andrewmclachlan/mooapp";
import { Chart as ChartJS, registerables } from "chart.js";

import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { PeriodSelector } from "components/PeriodSelector";
import { ReportTypeSelector } from "components/ReportTypeSelector";
import { Period } from "helpers/dateFns";
import { useParams } from "react-router";
import { Breakdown } from "./Breakdown";
import { transactionTypeFilter } from "store/state";

ChartJS.register(...registerables);

export const BreakdownPage = () => {

    const { id: accountId, tagId } = useParams<{ id: string, tagId: string }>();

    const [reportType, setReportType] = useState<transactionTypeFilter>("Debit");
    const [period, setPeriod] = useState<Period>({ startDate: null, endDate: null });
    const [selectedTagId, setSelectedTagId] = useState<number | undefined>(undefined);
    const [previousTagIds, setPreviousTagIds] = useState<number[]>([]);
    const tag = useTag(selectedTagId ?? 0);

    useEffect(() => {
        if (selectedTagId !== undefined) {
            setPreviousTagIds([...previousTagIds, selectedTagId]);
        }
        setSelectedTagId(tagId ? parseInt(tagId) : undefined);
    }, [tagId]);

    const selectedTagChanged = (tagId: number) => {
        if (selectedTagId !== undefined) {
            setPreviousTagIds([...previousTagIds, selectedTagId]);
        }
        setSelectedTagId(tagId);
    }

    const goBack = () => {
        console.debug("Previous Tag IDs: ", previousTagIds);
        setSelectedTagId(previousTagIds.pop());
        setPreviousTagIds([...previousTagIds]);
    }


    return (
        <ReportsPage title="Breakdown">
            <Section className="report-filter-panel">
                <ReportTypeSelector value={reportType} onChange={setReportType} />
                <PeriodSelector onChange={setPeriod} />
            </Section>
            <Section className="report doughnut">
                {tag.data?.name && <h3><FontAwesomeIcon className="clickable" icon="circle-chevron-left" size="xs" onClick={goBack} /> {tag.data.name}</h3>}
                {!tag.data && <h3>Top-Level Tags</h3>}
                <Breakdown accountId={accountId} tagId={selectedTagId} period={period} reportType={reportType} selectedTagChanged={selectedTagChanged} />
            </Section>
        </ReportsPage>
    );
};

BreakdownPage.displayName = "BreakdownPage";
