import React, { useEffect, useState } from "react";
import { useParams } from "@tanstack/react-router";

import type { ChartData } from "chart.js";
import { Bar } from "react-chartjs-2";

import { Section, useLocalStorage } from "@andrewmclachlan/moo-ds";

import { Amount } from "components";
import { TagSelector } from "components/TagSelector";
import { MiniPeriodSelector } from "components/MiniPeriodSelector";
import { getPeriod } from "hooks";
import { useTags } from "hooks/useTags";
import type { Period } from "models/dateFns";
import { useChartColours } from "utils/chartColours";
import { useSavingsInterestReport } from "../../../-hooks/useSavingsInterestReport";
import { ReportsPage } from "./ReportsPage";


export const SavingsInterest: React.FC = () => {

    const colours = useChartColours();

    const { id: accountId } = useParams({ strict: false });

    const [tagId, setTagId] = useLocalStorage<number | null>(`report-tag-${accountId}-interest`, null);
    const [period, setPeriod] = useState<Period>(getPeriod());

    const tags = useTags();

    useEffect(() => {
        if (tagId !== null || !tags.data) return;
        const interestTag = tags.data.find(t => t.name.trim().toLowerCase() === "interest");
        if (interestTag) setTagId(interestTag.id);
    }, [tagId, tags.data, setTagId]);

    const report = useSavingsInterestReport(accountId!, period?.startDate, period?.endDate, tagId ?? undefined);

    const tagChanged = (id: number | number[]) => {
        setTagId(typeof id === "number" ? id : id[0] ?? null);
    };

    const dataset: ChartData<"bar", number[], string> = {
        labels: report.data?.months.map(m => m.month) ?? [],
        datasets: [{
            label: "Interest",
            data: report.data?.months.map(m => m.grossAmount) ?? [],
            backgroundColor: colours.income,
        }],
    };

    return (
        <ReportsPage title="Interest Received" kind="SavingsInterest">
            <Section className="mini-filter-panel">
                <MiniPeriodSelector onChange={setPeriod} instant />
                <TagSelector value={tagId ?? undefined} onChange={tagChanged} id="interest-tag" />
            </Section>
            {!tagId &&
                <Section className="report" header="Interest Received" headerSize={3}>
                    <p>Select the tag you use for interest transactions on this account to see the chart.</p>
                </Section>
            }
            {tagId &&
                <>
                    <Section className="report" header="Monthly Interest" headerSize={3}>
                        <Bar id="savings-interest" data={dataset} options={{
                            maintainAspectRatio: true,
                            scales: {
                                y: {
                                    suggestedMin: 0,
                                    grid: { color: colours.grid },
                                },
                                x: {
                                    grid: { color: colours.grid },
                                },
                            },
                        }} />
                    </Section>
                    <Section className="averages">
                        <p>Total: <Amount amount={report.data?.total ?? 0} prefix="$" /></p>
                        <p>Monthly Average: <Amount amount={report.data?.monthlyAverage ?? 0} prefix="$" /></p>
                    </Section>
                </>
            }
        </ReportsPage>
    );
};

SavingsInterest.displayName = "SavingsInterest";
