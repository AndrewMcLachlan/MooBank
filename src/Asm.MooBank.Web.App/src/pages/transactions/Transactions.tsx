import React, { ElementType } from "react";
import { Col, Form, Modal } from "react-bootstrap";

import { IconButton, IconLinkButton, SectionRow, useLocalStorage } from "@andrewmclachlan/mooapp";

import { useAccount } from "components";
import { AccountPage, AccountSummary } from "../../components";
import { FilterPanel } from "./FilterPanel";
import { TransactionList } from "./TransactionList";

import { UpDownArrow } from "@andrewmclachlan/mooicons";
import { useAccountRoute } from "hooks/useAccountRoute";
import { Import } from "./Import";
import { MiniFilterPanel } from "./MiniFilterPanel";
import { AddTransaction } from "./AddTransaction";


export const Transactions: React.FC = () => {

    const account = useAccount();

    const [showImport, setShowImport] = React.useState(false);
    const [compactMode, setCompactMode] = useLocalStorage("compact-mode", false);
    const [show, setShow] = React.useState(false);

    if (!account) return null;

    let actions: React.ReactNode[] = [<Form.Switch key="compact" id="compact-mode" checked={compactMode} onChange={() => setCompactMode(!compactMode)} label="Compact" />,];

    switch (account.controller) {
        case "Manual":
        case "Virtual":
            actions = [
                ...actions,
                <IconButton key="add" variant="primary" icon="plus" onClick={() => setShow(true)}>Add Transaction</IconButton>,
            ];
            break;
        case "Import":
            actions = [
                ...actions,
                <IconButton key="import" variant="primary" icon="upload" onClick={() => setShowImport(true)}>Import Transactions</IconButton>,
            ];
            break;
        default:
            break;
    }

    return (
        <AccountPage title="Transactions" actions={actions}>
            <AddTransaction show={show} onClose={() => setShow(false) } balanceUpdate={false} />
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
            <Import show={showImport} accountId={account.id} onClose={() => setShowImport(false)} />
        </AccountPage>
    );
}
