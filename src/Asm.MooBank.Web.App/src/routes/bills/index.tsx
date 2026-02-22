import React, { useState } from "react";
import { createFileRoute } from "@tanstack/react-router";
import { Nav, Tab, Tabs } from "@andrewmclachlan/moo-ds";
import { useNavigate } from "@tanstack/react-router";

import { IconButton, Section } from "@andrewmclachlan/moo-ds";

import type { UtilityType } from "api/types.gen";
import { UtilityTypes } from "models/bills";
import { useBillAccountSummaries } from "./-hooks/useBillAccountSummaries";
import { UtilityTypeBillsTab } from "./-components/UtilityTypeBillsTab";
import { BillsPage } from "./-components/BillsPage";
import { AddBill } from "./-components/AddBill";

export const Route = createFileRoute("/bills/")({
    component: BillAccountSummaries,
});

function BillAccountSummaries() {
    const navigate = useNavigate();
    const { data: summaries } = useBillAccountSummaries();

    const availableTypes = summaries?.map(s => s.utilityType) ?? [];
    const [activeTab, setActiveTab] = useState<UtilityType | undefined>(
        availableTypes.length > 0 ? availableTypes[0] : undefined
    );
    const [showAddBill, setShowAddBill] = useState(false);

    React.useEffect(() => {
        if (!activeTab && availableTypes.length > 0) {
            setActiveTab(availableTypes[0]);
        }
    }, [availableTypes, activeTab]);

    return (
        <BillsPage
            title="Utilities"
            actions={availableTypes.length > 0 ? [
                <IconButton key="add" onClick={() => setShowAddBill(true)} icon="plus">Add Bill</IconButton>
            ] : []}
        >
            <AddBill show={showAddBill} onHide={() => setShowAddBill(false)} />
            {availableTypes.length === 0 ? (
                <Section>
                    <p className="empty-state">No utility accounts found. Create an account to get started.</p>
                    <IconButton onClick={() => navigate({ to: "/bills/accounts/create" })} icon="plus">Add Account</IconButton>
                </Section>
            ) : (
                <Tabs activeKey={activeTab} onSelect={(k) => setActiveTab(k as UtilityType)} >
                        {UtilityTypes.filter(type => availableTypes.includes(type)).map(type => (
                            <Tab key={type} eventKey={type} title={type}>
                                <UtilityTypeBillsTab utilityType={type} />
                            </Tab>
                        ))}
                </Tabs>
            )}
        </BillsPage>
    );
}
