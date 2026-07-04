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
import { useSavingsInterestReport } from "../../../-hooks/useSavingsInterestReport";
import { ReportsPage } from "./ReportsPage";


export const SavingsInterest: React.FC = () => {

    const colours = useChartColours();

    const account = useAccount() as LogicalAccount;
    const interestTagId = account?.tagPurposes?.find(t => t.purpose === "Interest")?.tagId;

    const [period, setPeriod] = useState<Period>(getPeriod());

    const report = useSavingsInterestReport(account?.id ?? "", period?.startDate, period?.endDate, !!interestTagId);

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
            </Section>
            {!interestTagId &&
                <Section className="report" header="Interest Received" headerSize={3}>
                    <p>No interest tag is configured for this account. Set one in <a href={`/accounts/${account?.id}/manage`}>account settings</a> to see this report.</p>
                </Section>
            }
            {interestTagId &&
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
