import React, { ElementType } from "react";
import { Col, Modal } from "react-bootstrap";

import { IconButton, IconLinkButton, SectionRow } from "@andrewmclachlan/mooapp";

import { useAccount } from "components";
import { AccountPage, AccountSummary } from "../../components";
import { FilterPanel } from "./FilterPanel";
import { TransactionList } from "./TransactionList";

import { UpDownArrow } from "@andrewmclachlan/mooicons";
import { useAccountRoute } from "hooks/useAccountRoute";
import { Import } from "./Import";


export const Transactions: React.FC = () => {

    const account = useAccount();
    const route = useAccountRoute();

    const [showImport, setShowImport] = React.useState(false);

    if (!account) return null;

    let actions: React.ReactNode[] = [];

    switch (account.controller) {
        case "Manual":
            actions = [
                <IconLinkButton key="add" variant="primary" icon="plus" to={`${route}/transactions/add`} relative="route">Add Transaction</IconLinkButton>,
                <IconLinkButton key="adjust" variant="primary" icon={UpDownArrow as ElementType} to={`${route}/balance`} relative="route">Adjust balance</IconLinkButton>
            ];
            break;
        case "Import":
            actions = [
                <IconButton key="import" variant="primary" icon="upload" onClick={() => setShowImport(true)}>Import Transactions</IconButton>,
            ];
            break;
        default:
            actions = [];
    }

    return (
        <AccountPage title="Transactions" actions={actions}>
            <SectionRow>
                <Col xxl={3} xl={12} lg={12} md={12} sm={12}>
                    <AccountSummary />
                </Col>
                <Col xxl={9} xl={12} lg={12} md={12} sm={12}>
                    <FilterPanel />
                </Col>
            </SectionRow>
            <TransactionList />
            <Import show={showImport} accountId={account.id} onClose={() => setShowImport(false)} />
        </AccountPage>
    );
}
