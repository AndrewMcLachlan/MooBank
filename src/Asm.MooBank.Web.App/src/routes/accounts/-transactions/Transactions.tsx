import React from "react";
import { Col, Input } from "@andrewmclachlan/moo-ds";

import { IconButton, SectionRow, useLocalStorage } from "@andrewmclachlan/moo-ds";

import { useAccount } from "components";
import { AccountPage, AccountSummary } from "components";
import { FilterPanel } from "./components/FilterPanel";
import { TransactionList } from "./components/TransactionList";

import { Import } from "./components/Import";
import { MiniFilterPanel } from "./components/MiniFilterPanel";
import { AddTransaction } from "./components/AddTransaction";
import { useTransactionList } from "components";


export const Transactions: React.FC = () => {

    const account = useAccount();

    const [showImport, setShowImport] = React.useState(false);
    const [compactMode, setCompactMode] = useLocalStorage("compact-mode", false);
    const { showNet, setShowNet } = useTransactionList();
    const [show, setShow] = React.useState(false);

    if (!account) return null;

    let actions: React.ReactNode[] = [
        <Input.Switch key="show-net" id="show-net-amount" checked={showNet} onChange={() => setShowNet(!showNet)} label="Show Net Amount" />,
        <Input.Switch key="compact" id="compact-mode" checked={compactMode} onChange={() => setCompactMode(!compactMode)} label="Compact" />,
    ];

    switch (account.controller) {
        case "Manual":
        case "Virtual":
            actions = [
                ...actions,
                <IconButton key="add" variant="primary" icon="plus" onClick={() => setShow(true)}>Add</IconButton>,
            ];
            break;
        case "Import":
            actions = [
                ...actions,
                <IconButton key="import" variant="primary" icon="upload" onClick={() => setShowImport(true)}>Import</IconButton>,
            ];
            break;
        default:
            break;
    }

    return (
        <AccountPage title="Transactions" actions={actions}>
            <AddTransaction show={show} onClose={() => setShow(false)} onSave={() => setShow(false)} balanceUpdate={false} />
            <SectionRow hidden={compactMode}>
                <Col xxl={3} xl={12} lg={12} md={12} sm={12}>
                    <AccountSummary />
                </Col>
                <Col xxl={9} xl={12} lg={12} md={12} sm={12}>
                    <FilterPanel />
                </Col>
            </SectionRow>
            <MiniFilterPanel hidden={!compactMode} />
            <TransactionList />
            <TransactionList compact />
            {account.controller === "Import" && <Import show={showImport} accountId={account.id} onClose={() => setShowImport(false)} />}
        </AccountPage>
    );
}
