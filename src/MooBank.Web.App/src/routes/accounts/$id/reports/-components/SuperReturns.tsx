import React, { useState } from "react";

import { Section } from "@andrewmclachlan/moo-ds";

import type { LogicalAccount } from "api/types.gen";
import { Amount } from "components";
import { useAccount } from "components/AccountProvider";
import { MiniPeriodSelector } from "components/MiniPeriodSelector";
import { getPeriod } from "hooks";
import type { Period } from "models/dateFns";
import { useSuperReturnsReport } from "../../../-hooks/useSuperReturnsReport";
import { ReportsPage } from "./ReportsPage";


export const SuperReturns: React.FC = () => {

    const account = useAccount() as LogicalAccount;
    const employerTagId = account?.tagPurposes?.find(t => t.purpose === "EmployerContribution")?.tagId;
    const personalTagId = account?.tagPurposes?.find(t => t.purpose === "PersonalContribution")?.tagId;
    const anyConfigured = !!employerTagId || !!personalTagId;

    const [period, setPeriod] = useState<Period>(getPeriod());
    const report = useSuperReturnsReport(account?.id ?? "", period?.startDate, period?.endDate, anyConfigured);

    const years = report.data?.years ?? [];

    return (
        <ReportsPage title="Annual Returns" kind="SuperReturns">
            <Section className="mini-filter-panel">
                <MiniPeriodSelector onChange={setPeriod} instant />
            </Section>
            {!anyConfigured &&
                <Section className="report" header="Annual Returns" headerSize={3}>
                    <p>No contribution tags are configured. Set the Employer and/or Personal contribution tags in <a href={`/accounts/${account?.id}/manage`}>account settings</a> so contributions can be excluded from the implied return.</p>
                </Section>
            }
            {anyConfigured &&
                <Section className="report" header="By Financial Year" headerSize={3}>
                    <table className="returns-table">
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
                                    <td><Amount amount={y.openingBalance} prefix="$" /></td>
                                    <td><Amount amount={y.contributions} prefix="$" /></td>
                                    <td><Amount amount={y.closingBalance} prefix="$" /></td>
                                    <td><Amount amount={y.return} prefix="$" positiveColour negativeColour /></td>
                                    <td>{y.returnPercent !== null && y.returnPercent !== undefined ? `${y.returnPercent}%` : "—"}</td>
                                </tr>
                            ))}
                            {years.length === 0 && (
                                <tr><td colSpan={6}>No data for the selected period.</td></tr>
                            )}
                        </tbody>
                    </table>
                </Section>
            }
        </ReportsPage>
    );
};

SuperReturns.displayName = "SuperReturns";
