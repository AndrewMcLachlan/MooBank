import { useEffect, useState } from "react";

import { useTag } from "../../../-hooks/useTag";
import { ReportsPage } from "./ReportsPage";

import { Section } from "@andrewmclachlan/moo-ds";
import { Chart as ChartJS, registerables } from "chart.js";

import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { ReportTypeSelector } from "components/ReportTypeSelector";
import { Period } from "helpers/dateFns";
import { useNavigate, useParams } from "@tanstack/react-router";
import { Breakdown } from "./Breakdown";
import { transactionTypeFilter } from "store/state";
import { TagValue } from "api/types.gen";
import { MiniPeriodSelector } from "components/MiniPeriodSelector";
import { getPeriod } from "hooks";

ChartJS.register(...registerables);

export const BreakdownPage = () => {

    const { id: accountId, tagId } = useParams({ strict: false });

    const navigate = useNavigate();

    const [reportType, setReportType] = useState<transactionTypeFilter>("Debit");
    const [period, setPeriod] = useState<Period>(getPeriod());
    const [selectedTagId, setSelectedTagId] = useState<number | undefined>(undefined);
    const [previousTagIds, setPreviousTagIds] = useState<number[]>([]);
    const tag = useTag(selectedTagId ?? 0);

    useEffect(() => {
        if (selectedTagId !== undefined) {
            setPreviousTagIds([...previousTagIds, selectedTagId]);
        }
        setSelectedTagId(tagId ? parseInt(tagId) : undefined);
    }, [tagId]);

    const selectedTagChanged = (tag: TagValue) => {
        if (selectedTagId !== undefined) {
            setPreviousTagIds([...previousTagIds, selectedTagId]);
        }
        setSelectedTagId(tag.tagId ?? undefined);


        if (!tag.hasChildren || tag.tagId === selectedTagId) {

            const url = !tag.tagId ? `/accounts/${accountId}?untagged=true` : `/accounts/${accountId}?tag=${tag.tagId}&type=${reportType}`;

            navigate({ to: url });
            return;
        }

        const newUrl = `/accounts/${accountId}/reports/breakdown/${tagId}`;
        window.history.replaceState({ path: newUrl }, "", newUrl);
    }

    const goBack = () => {
        console.debug("Previous Tag IDs: ", previousTagIds);
        setSelectedTagId(previousTagIds.pop());
        setPreviousTagIds([...previousTagIds]);
    }


    return (
        <ReportsPage title="Breakdown">
            <Section className="mini-filter-panel">
                <ReportTypeSelector value={reportType} onChange={setReportType} />
                <MiniPeriodSelector onChange={setPeriod} />
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
