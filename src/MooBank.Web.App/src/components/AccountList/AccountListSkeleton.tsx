import React from "react";
import { SectionTable, Skeleton } from "@andrewmclachlan/moo-ds";

// Loading placeholder for the account list. The formatted-accounts query is usually already warm
// from the dashboard, so this mainly covers a cold/direct visit to /accounts. It mirrors
// AccountListGroup's table (same columns and responsive classes) so nothing shifts when data
// arrives. Each table carries aria-busy; the Skeleton shapes are aria-hidden, so the loading region
// owns the single busy announcement. Cell widths auto-vary via moo-ds's tr-scoped skeleton CSS.
const SkeletonGroup: React.FC<{ rows: number }> = ({ rows }) => (
    <SectionTable
        className="accounts"
        hover
        aria-busy="true"
        aria-label="Loading accounts"
        header={<header><h3><Skeleton.Text style={{ width: "12rem" }} /></h3></header>}
        headerSize={2}
    >
        <thead>
            <tr>
                <th className="expander d-none d-sm-table-cell"></th>
                <th>Name</th>
                <th className="d-none d-sm-table-cell">Type</th>
                <th className="number">Balance</th>
            </tr>
        </thead>
        <tbody>
            {Array.from({ length: rows }, (_, i) => (
                <tr key={i}>
                    <td className="expander d-none d-sm-table-cell"></td>
                    <td><Skeleton.Text /></td>
                    <td className="d-none d-sm-table-cell"><Skeleton.Text /></td>
                    <td className="number"><Skeleton.Text /></td>
                </tr>
            ))}
        </tbody>
    </SectionTable>
);

export const AccountListSkeleton: React.FC = () => (
    <>
        <SkeletonGroup rows={3} />
        <SkeletonGroup rows={5} />
    </>
);

AccountListSkeleton.displayName = "AccountListSkeleton";
