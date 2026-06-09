import React, { useState } from "react";

import type { ChartData } from "chart.js";
import { Bar } from "react-chartjs-2";

import { Section } from "@andrewmclachlan/moo-ds";

import type { LogicalAccount } from "api/types.gen";
import { Amount } from "components";
import { useAccount } from "components/AccountProvider";
import { MiniPeriodSelector } from "components/MiniPeriodSelector";
import { getPeriod } from "hooks";
import type { Period } from "models/dateFns";
import { useChartColours } from "utils/chartColours";
import { useSuperContributionsReport } from "../../../-hooks/useSuperContributionsReport";
import { ReportsPage } from "./ReportsPage";


export const SuperContributions: React.FC = () => {

    const colours = useChartColours();

    const account = useAccount() as LogicalAccount;
    const employerTagId = account?.tagPurposes?.find(t => t.purpose === "EmployerContribution")?.tagId;
    const personalTagId = account?.tagPurposes?.find(t => t.purpose === "PersonalContribution")?.tagId;
    const anyConfigured = !!employerTagId || !!personalTagId;

    const [period, setPeriod] = useState<Period>(getPeriod());
    const report = useSuperContributionsReport(account?.id ?? "", period?.startDate, period?.endDate, anyConfigured);

    const months = new Set<string>();
    report.data?.employer.forEach(p => months.add(p.month));
    report.data?.personal.forEach(p => months.add(p.month));
    const sortedMonths = [...months].sort();

    const monthValue = (series: { month: string; grossAmount: number }[] | undefined, month: string) =>
        series?.find(p => p.month === month)?.grossAmount ?? 0;

    const dataset: ChartData<"bar", number[], string> = {
        labels: sortedMonths,
        datasets: [
            {
                label: report.data?.employerTagName ?? "Employer",
                data: sortedMonths.map(m => monthValue(report.data?.employer, m)),
                backgroundColor: colours.income,
            },
            {
                label: report.data?.personalTagName ?? "Personal",
                data: sortedMonths.map(m => monthValue(report.data?.personal, m)),
                backgroundColor: colours.expenses,
            },
        ],
    };

    return (
        <ReportsPage title="Contributions" kind="SuperContributions">
            <Section className="mini-filter-panel">
                <MiniPeriodSelector onChange={setPeriod} instant />
            </Section>
            {!anyConfigured &&
                <Section className="report" header="Contributions" headerSize={3}>
                    <p>No contribution tags are configured. Set the Employer and/or Personal contribution tags in <a href={`/accounts/${account?.id}/manage`}>account settings</a> to see this report.</p>
                </Section>
            }
            {anyConfigured &&
                <>
                    <Section className="report" header="Monthly Contributions" headerSize={3}>
                        <Bar id="super-contributions" data={dataset} options={{
                            maintainAspectRatio: true,
                            scales: {
                                x: { stacked: true, grid: { color: colours.grid } },
                                y: { stacked: true, suggestedMin: 0, grid: { color: colours.grid } },
                            },
                        }} />
                    </Section>
                    <Section className="averages">
                        <p>Employer total: <Amount amount={report.data?.employerTotal ?? 0} currencyCode={account?.currency} /></p>
                        <p>Personal total: <Amount amount={report.data?.personalTotal ?? 0} currencyCode={account?.currency} /></p>
                    </Section>
                </>
            }
        </ReportsPage>
    );
};

SuperContributions.displayName = "SuperContributions";
