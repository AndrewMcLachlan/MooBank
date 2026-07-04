import React from "react";

import { Section } from "@andrewmclachlan/moo-ds";

import type { LogicalAccount, TagPurpose } from "api/types.gen";
import { TagSelector } from "components/TagSelector";
import { useSetAccountTagPurpose } from "routes/accounts/-hooks/useSetAccountTagPurpose";

const labels: Record<TagPurpose, string> = {
    Interest: "Interest tag",
    EmployerContribution: "Employer contribution tag",
    PersonalContribution: "Personal contribution tag",
    MortgageInterest: "Mortgage interest tag",
};

const descriptions: Record<TagPurpose, string> = {
    Interest: "Tag that identifies interest credits on this account. Used by the Interest Received report.",
    EmployerContribution: "Tag that identifies employer contributions. Used by the Super Contributions report.",
    PersonalContribution: "Tag that identifies personal contributions. Used by the Super Contributions report.",
    MortgageInterest: "Tag that identifies interest portion of mortgage payments. Used by the Principal vs Interest report.",
};

export const ReportTagSettings: React.FC<{ account: LogicalAccount | undefined }> = ({ account }) => {

    const { set } = useSetAccountTagPurpose();

    if (!account) return null;
    const available = account.availableTagPurposes ?? [];
    if (available.length === 0) return null;

    const tagFor = (purpose: TagPurpose): number | undefined =>
        account.tagPurposes?.find(t => t.purpose === purpose)?.tagId;

    const change = (purpose: TagPurpose, value: number | number[]) => {
        const tagId = typeof value === "number" ? value : value[0];
        set(account.id, purpose, tagId ?? null);
    };

    return (
        <Section header="Report Tags" headerSize={3}>
            {available.map(purpose => (
                <div key={purpose} className="report-tag-row">
                    <label htmlFor={`tag-purpose-${purpose}`}>{labels[purpose]}</label>
                    <TagSelector
                        id={`tag-purpose-${purpose}`}
                        value={tagFor(purpose)}
                        onChange={value => change(purpose, value)}
                    />
                    <small>{descriptions[purpose]}</small>
                </div>
            ))}
        </Section>
    );
};

ReportTagSettings.displayName = "ReportTagSettings";
