import React, { useState } from "react";
import { createFileRoute } from "@tanstack/react-router";
import { Tab, Tabs } from "@andrewmclachlan/moo-ds";
import { useNavigate } from "@tanstack/react-router";
import { validatePageSearch } from "utils/gridSearch";

import { IconButton, Section, Skeleton } from "@andrewmclachlan/moo-ds";

import type { UtilityType } from "api/types.gen";
import { UtilityTypes } from "models/bills";
import { useBillAccountSummaries } from "./-hooks/useBillAccountSummaries";
import { UtilityTypeBillsTab } from "./-components/UtilityTypeBillsTab";
import { BillsPage } from "./-components/BillsPage";
import { AddBill } from "./-components/AddBill";

export interface BillsSearch {
    type?: UtilityType;
    page?: number;
}

export const Route = createFileRoute("/bills/")({
    validateSearch: (search: Record<string, unknown>): BillsSearch => {
        const type = UtilityTypes.find(t => t === search.type);
        return { ...(type ? { type } : {}), ...validatePageSearch(search) };
    },
    component: BillAccountSummaries,
});

const BillsLoading: React.FC = () => (
    <>
        <div className="bills-loading-tabs">
            <Skeleton.Rect />
            <Skeleton.Rect />
            <Skeleton.Rect />
        </div>
        <Section className="bill-chart">
            <Skeleton.Chart variant="bar" count={8} />
        </Section>
        <Section className="bill-chart">
            <Skeleton.Chart variant="line" count={2} />
        </Section>
    </>
);

function BillAccountSummaries() {
    const navigate = useNavigate();
    const { data: summaries, isLoading } = useBillAccountSummaries();

    const { type } = Route.useSearch();
    const [showAddBill, setShowAddBill] = useState(false);

    const availableTypes = summaries?.map(s => s.utilityType) ?? [];
    const tabs = UtilityTypes.filter(t => availableTypes.includes(t));
    const activeTab = tabs.includes(type) ? type : tabs[0];

    const selectTab = (key: UtilityType) =>
        navigate({ to: Route.fullPath, search: { type: key === tabs[0] ? undefined : key } as any });

    return (
        <BillsPage
            title="Utilities"
            actions={availableTypes.length > 0 ? [
                { id: "add", label: "Add Bill", icon: "plus", group: "write", onClick: () => setShowAddBill(true) },
            ] : []}
        >
            <AddBill show={showAddBill} onHide={() => setShowAddBill(false)} />
            {isLoading ? (
                <BillsLoading />
            ) : availableTypes.length === 0 ? (
                <Section>
                    <p className="empty-state">No utility accounts found. Create an account to get started.</p>
                    <IconButton badge onClick={() => navigate({ to: "/bills/accounts/create" })} icon="plus">Add Account</IconButton>
                </Section>
            ) : (
                <Tabs activeKey={activeTab} onSelect={(k) => selectTab(k as UtilityType)} >
                        {tabs.map(t => (
                            <Tab key={t} eventKey={t} title={t}>
                                <UtilityTypeBillsTab utilityType={t} />
                            </Tab>
                        ))}
                </Tabs>
            )}
        </BillsPage>
    );
}
