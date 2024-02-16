import React from "react";
import { Col, Row } from "react-bootstrap";

import { IconLinkButton, SectionRow } from "@andrewmclachlan/mooapp";

import { useAccount } from "components";
import { AccountPage, AccountSummary } from "../../components";
import { FilterPanel } from "./FilterPanel";
import { TransactionList } from "./TransactionList";

import { useAccountRoute } from "hooks/useAccountRoute";
import { InstitutionAccount } from "models";


export const Transactions: React.FC = () => {

    const account = useAccount();
    const route = useAccountRoute();

    if (!account) return null;

    const actions = !(account as InstitutionAccount).importerTypeId ? [<IconLinkButton key="import" variant="primary" icon="plus" to={`${route}/transactions/add`} relative="route">Add Transaction</IconLinkButton>] : [];

    return (
        <AccountPage title="Transactions" actions={actions}>
            <SectionRow>
                <Col xl={2} md={12} sm={12}>
                    <AccountSummary />
                </Col>
                <Col xl={10} md={12} sm={12}>
                    <FilterPanel />
                </Col>
            </SectionRow>

            <TransactionList />
        </AccountPage>
    );
}
