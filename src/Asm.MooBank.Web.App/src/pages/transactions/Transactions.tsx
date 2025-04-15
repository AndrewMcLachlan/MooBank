import React, { ElementType } from "react";
import { Col } from "react-bootstrap";

import { IconLinkButton, SectionRow } from "@andrewmclachlan/mooapp";

import { useAccount } from "components";
import { AccountPage, AccountSummary } from "../../components";
import { FilterPanel } from "./FilterPanel";
import { TransactionList } from "./TransactionList";

import { UpDownArrow } from "@andrewmclachlan/mooicons";
import { useAccountRoute } from "hooks/useAccountRoute";
import { InstitutionAccount } from "models";


export const Transactions: React.FC = () => {

    const account = useAccount();
    const route = useAccountRoute();

    if (!account) return null;

    const actions = !(account as InstitutionAccount).importerTypeId ? [
        <IconLinkButton key="add" variant="primary" icon="plus" to={`${route}/transactions/add`} relative="route">Add Transaction</IconLinkButton>,
        <IconLinkButton key="adjust" variant="primary" icon={UpDownArrow as ElementType} to={`${route}/balance`} relative="route">Adjust balance</IconLinkButton>
    ] : [];

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
        </AccountPage>
    );
}
