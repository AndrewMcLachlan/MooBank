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
import { usePrincipalVsInterestReport } from "../../../-hooks/usePrincipalVsInterestReport";
import { ReportsPage } from "./ReportsPage";


export const PrincipalVsInterest: React.FC = () => {

    const colours = useChartColours();

    const account = useAccount() as LogicalAccount;
    const interestTagId = account?.tagPurposes?.find(t => t.purpose === "MortgageInterest")?.tagId;

    const [period, setPeriod] = useState<Period>(getPeriod());
    const report = usePrincipalVsInterestReport(account?.id ?? "", period?.startDate, period?.endDate, !!interestTagId);

    const months = new Set<string>();
    report.data?.interest.forEach(p => months.add(p.month));
    report.data?.principal.forEach(p => months.add(p.month));
    const sortedMonths = [...months].sort();

    const monthValue = (series: { month: string; grossAmount: number }[] | undefined, month: string) =>
        series?.find(p => p.month === month)?.grossAmount ?? 0;

    const dataset: ChartData<"bar", number[], string> = {
        labels: sortedMonths,
        datasets: [
            {
                label: "Principal",
                data: sortedMonths.map(m => monthValue(report.data?.principal, m)),
                backgroundColor: colours.income,
            },
            {
                label: report.data?.interestTagName ?? "Interest",
                data: sortedMonths.map(m => monthValue(report.data?.interest, m)),
                backgroundColor: colours.expenses,
            },
        ],
    };

    return (
        <ReportsPage title="Principal vs Interest" kind="PrincipalVsInterest">
            <Section className="mini-filter-panel">
                <MiniPeriodSelector onChange={setPeriod} instant />
            </Section>
            {!interestTagId &&
                <Section className="report" header="Principal vs Interest" headerSize={3}>
                    <p>No mortgage interest tag is configured. Set one in <a href={`/accounts/${account?.id}/manage`}>account settings</a> to see this report. Principal is derived as the rest of each month's debits.</p>
                </Section>
            }
            {interestTagId &&
                <>
                    <Section className="report" header="Monthly Split" headerSize={3}>
                        <Bar id="principal-vs-interest" data={dataset} options={{
                            maintainAspectRatio: true,
                            scales: {
                                x: { stacked: true, grid: { color: colours.grid } },
                                y: { stacked: true, suggestedMin: 0, grid: { color: colours.grid } },
                            },
                        }} />
                    </Section>
                    <Section className="averages">
                        <p>Principal total: <Amount amount={report.data?.principalTotal ?? 0} currencyCode={account?.currency} /></p>
                        <p>Interest total: <Amount amount={report.data?.interestTotal ?? 0} currencyCode={account?.currency} /></p>
                    </Section>
                </>
            }
        </ReportsPage>
    );
};

PrincipalVsInterest.displayName = "PrincipalVsInterest";
