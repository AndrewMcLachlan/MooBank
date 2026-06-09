import React from "react";

import type { ChartData } from "chart.js";
import { Bar } from "react-chartjs-2";

import { Section, SectionTable } from "@andrewmclachlan/moo-ds";

import type { LogicalAccount } from "api/types.gen";
import { Amount } from "components";
import { useAccount } from "components/AccountProvider";
import { useChartColours } from "utils/chartColours";
import { useSuperReturnsReport } from "../../../-hooks/useSuperReturnsReport";
import { ReportsPage } from "./ReportsPage";


export const SuperReturns: React.FC = () => {

    const colours = useChartColours();

    const account = useAccount() as LogicalAccount;
    const employerTagId = account?.tagPurposes?.find(t => t.purpose === "EmployerContribution")?.tagId;
    const personalTagId = account?.tagPurposes?.find(t => t.purpose === "PersonalContribution")?.tagId;
    const anyConfigured = !!employerTagId || !!personalTagId;

    const report = useSuperReturnsReport(account?.id ?? "", anyConfigured);
    const years = report.data?.years ?? [];

    const chartData: ChartData<"bar", number[], string> = {
        labels: years.map(y => `FY${y.financialYear}`),
        datasets: [{
            label: "Return",
            data: years.map(y => y.return),
            backgroundColor: years.map(y => y.return >= 0 ? colours.income : colours.expenses),
        }],
    };

    return (
        <ReportsPage title="Annual Returns" kind="SuperReturns">
            {!anyConfigured &&
                <Section className="report" header="Annual Returns" headerSize={3}>
                    <p>No contribution tags are configured. Set the Employer and/or Personal contribution tags in <a href={`/accounts/${account?.id}/manage`}>account settings</a> so contributions can be excluded from the implied return.</p>
                </Section>
            }
            {anyConfigured && years.length > 0 &&
                <Section className="report" header="Return by Financial Year" headerSize={3}>
                    <Bar id="super-returns-chart" data={chartData} options={{
                        maintainAspectRatio: true,
                        plugins: { legend: { display: false } },
                        scales: {
                            x: { grid: { color: colours.grid } },
                            y: { grid: { color: colours.grid } },
                        },
                    }} />
                </Section>
            }
            {anyConfigured &&
                <SectionTable header="By Financial Year" striped>
                    <thead>
                        <tr>
                            <th>FY</th>
                            <th>Opening</th>
                            <th>Contributions</th>
                            <th>Closing</th>
                            <th>Return</th>
                            <th>Return %</th>
                        </tr>
                    </thead>
                    <tbody>
                        {years.map(y => (
                            <tr key={y.financialYear}>
                                <td>FY{y.financialYear}</td>
                                <td><Amount amount={y.openingBalance} currencyCode={account?.currency} /></td>
                                <td><Amount amount={y.contributions} currencyCode={account?.currency} /></td>
                                <td><Amount amount={y.closingBalance} currencyCode={account?.currency} /></td>
                                <td><Amount amount={y.return} currencyCode={account?.currency} positiveColour negativeColour /></td>
                                <td>{y.returnPercent !== null && y.returnPercent !== undefined ? `${y.returnPercent}%` : "—"}</td>
                            </tr>
                        ))}
                        {years.length === 0 && (
                            <tr><td colSpan={6}>Not enough balance history yet — needs at least two annual entries.</td></tr>
                        )}
                    </tbody>
                </SectionTable>
            }
        </ReportsPage>
    );
};

SuperReturns.displayName = "SuperReturns";
