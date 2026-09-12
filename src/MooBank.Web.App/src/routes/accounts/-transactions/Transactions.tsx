import React from "react";
import { Col, useIsAtLeast } from "@andrewmclachlan/moo-ds";

import { SectionRow, useLocalStorage } from "@andrewmclachlan/moo-ds";

import { useAccount } from "components";
import { AccountPage } from "components";
import { FilterPanel } from "./components/FilterPanel";
import { TransactionList } from "./components/TransactionList";

import { Import } from "./components/Import";
import { MiniFilterPanel } from "./components/MiniFilterPanel";
import { AddTransaction } from "./components/AddTransaction";
import { TransactionsAccountCard } from "./components/TransactionsAccountCard";
import { TransactionsCompactWidgets } from "./components/TransactionsCompactWidgets";
import { TransactionsMobileHeader } from "./components/TransactionsMobileHeader";
import { TransactionsFilterBar } from "./components/TransactionsFilterBar";
import { useTransactionList } from "components";
import type { PageAction } from "@andrewmclachlan/moo-app";

export const Transactions: React.FC = () => {

    const account = useAccount();

    const [showImport, setShowImport] = React.useState(false);
    const [compactMode, setCompactMode] = useLocalStorage("compact-mode", false);
    const { showNet, setShowNet } = useTransactionList();
    const [show, setShow] = React.useState(false);
    const isPhone = !useIsAtLeast("md");

    if (!account) return null;

    let actions: PageAction[] = [
        { id: "show-net-amount", label: "Show Net Amount", checked: showNet, onClick: () => setShowNet(!showNet) },
    ];

    if (!isPhone) {
        actions = [...actions, { id: "compact-mode", label: "Compact", checked: compactMode, onClick: () => setCompactMode(!compactMode) }];
    }

    switch (account.controller) {
        case "Manual":
        case "Virtual":
            actions = [
                ...actions,
                { id: "add", label: "Add", icon: "plus", variant: "primary", group: "write", onClick: () => setShow(true) },
            ];
            break;
        case "Import":
            actions = [
                ...actions,
                { id: "import", label: "Import", icon: "upload", variant: "primary", group: "write", onClick: () => setShowImport(true) },
            ];
            break;
        default:
            break;
    }

    return (
        <AccountPage title="Transactions" actions={actions}>
            <AddTransaction show={show} onClose={() => setShow(false)} onSave={() => setShow(false)} />
            {isPhone ? (
                <>
                    <TransactionsMobileHeader />
                    <TransactionsFilterBar />
                </>
            ) : compactMode ? (
                <>
                    <TransactionsCompactWidgets />
                    <MiniFilterPanel />
                </>
            ) : (
                <SectionRow>
                    <Col xxl={5} xl={12} lg={12} md={12} sm={12}>
                        <TransactionsAccountCard />
                    </Col>
                    <Col xxl={7} xl={12} lg={12} md={12} sm={12}>
                        <FilterPanel />
                    </Col>
                </SectionRow>
            )}
            <TransactionList compact={isPhone} />
            {account.controller === "Import" && <Import show={showImport} accountId={account.id} onClose={() => setShowImport(false)} />}
        </AccountPage>
    );
}
