import { useState } from "react";

import { useTag } from "../../../-hooks/useTag";
import { ReportsPage } from "./ReportsPage";

import { Section } from "@andrewmclachlan/moo-ds";


import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { ReportTypeSelector } from "components/ReportTypeSelector";
import type { Period } from "models/dateFns";
import { useNavigate, useParams } from "@tanstack/react-router";
import { Breakdown } from "./Breakdown";
import type { transactionTypeFilter } from "store/state";
import type { TagValue } from "api/types.gen";
import { MiniPeriodSelector } from "components/MiniPeriodSelector";
import { getPeriod } from "hooks";


export const BreakdownPage = () => {

    const { id: accountId, tagId: tagIdParam } = useParams({ strict: false });
    const tagId = tagIdParam ? parseInt(tagIdParam) : undefined;

    const navigate = useNavigate();

    const [reportType, setReportType] = useState<transactionTypeFilter>("Debit");
    const [period, setPeriod] = useState<Period>(getPeriod());
    const [previousTagIds, setPreviousTagIds] = useState<number[]>([]);
    const tag = useTag(tagId);

    const selectedTagChanged = (clickedTag: TagValue) => {
        if (!clickedTag.hasChildren || clickedTag.tagId === tagId) {
            const url = !clickedTag.tagId
                ? `/accounts/${accountId}?untagged=true`
                : `/accounts/${accountId}?tag=${clickedTag.tagId}&type=${reportType}`;
            navigate({ to: url });
            return;
        }

        if (tagId !== undefined) {
            setPreviousTagIds([...previousTagIds, tagId]);
        }
        navigate({
            to: `/accounts/${accountId}/reports/breakdown/${clickedTag.tagId}`,
            replace: true,
        });
    };

    const goBack = () => {
        const previous = previousTagIds[previousTagIds.length - 1];
        setPreviousTagIds(previousTagIds.slice(0, -1));
        const url = previous !== undefined
            ? `/accounts/${accountId}/reports/breakdown/${previous}`
            : `/accounts/${accountId}/reports/breakdown`;
        navigate({ to: url, replace: true });
    };


    return (
        <ReportsPage title="Breakdown" kind="Breakdown">
            <Section className="mini-filter-panel">
                <ReportTypeSelector value={reportType} onChange={setReportType} />
                <MiniPeriodSelector onChange={setPeriod} />
            </Section>
            <Section className="report doughnut">
                {tag.data?.name && <h3><FontAwesomeIcon className="clickable" icon="circle-chevron-left" size="xs" onClick={goBack} /> {tag.data.name}</h3>}
                {!tag.data && <h3>Top-Level Tags</h3>}
                <Breakdown accountId={accountId} tagId={tagId} period={period} reportType={reportType} selectedTagChanged={selectedTagChanged} />
            </Section>
        </ReportsPage>
    );
};

BreakdownPage.displayName = "BreakdownPage";
