import React, { useState } from "react";
import { Nav, Tab } from "@andrewmclachlan/moo-ds";
import { useNavigate } from "react-router";

import { IconButton, Section } from "@andrewmclachlan/moo-ds";

import { UtilityType, UtilityTypes } from "models/bills";
import { useBillAccountSummaries } from "services";
import { UtilityTypeBillsTab } from "./components/UtilityTypeBillsTab";
import { BillsPage } from "./BillsPage";
import { AddBill } from "./bills/AddBill";

export const BillAccountSummaries: React.FC = () => {
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
                    <IconButton onClick={() => navigate("/bills/accounts/create")} icon="plus">Add Account</IconButton>
                </Section>
            ) : (
                <Tab.Container activeKey={activeTab} onSelect={(k) => setActiveTab(k as UtilityType)} >
                    {/* <Nav variant="tabs">
                        {UtilityTypes.filter(type => availableTypes.includes(type)).map(type => (
                            <Nav.Item key={type}>
                                <Nav.Link eventKey={type}>{type}</Nav.Link>
                            </Nav.Item>
                        ))}
                    </Nav> */}
                    <Tab.Content>
                        {UtilityTypes.filter(type => availableTypes.includes(type)).map(type => (
                            <Tab.Pane key={type} eventKey={type} title={type}>
                                <UtilityTypeBillsTab utilityType={type} />
                            </Tab.Pane>
                        ))}
                    </Tab.Content>
                </Tab.Container>
            )}
        </BillsPage>
    );
};

BillAccountSummaries.displayName = "BillAccountSummaries";
