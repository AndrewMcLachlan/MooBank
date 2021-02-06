import "./AccountSummary.scss";

import React from "react";

import { Account, AccountController } from "../models";
import { AccountBalance } from ".";
import { PageHeader } from "./PageHeader";
import { MenuItem } from "../models/MenuItem";
import { useAccount } from "./AccountProvider";
import { Table } from "react-bootstrap";

export const AccountSummary: React.FC<AccountSummaryProps> = (props) => {

    const account = useAccount();

    const { getMenuItems } = useRenderers(account);

    if (!account) return null;

    return (
        <>
            <PageHeader title={account.name} menuItems={getMenuItems()} />
            <Table>
                <tbody>
                    <tr>
                        <th>Balance</th>
                        <td><AccountBalance balance={account.currentBalance} /></td>
                    </tr>
                    {/*<tr>
                        <th>Available Balance</th>
                        <td><AccountBalance balance={account.availableBalance} /></td>
                    </tr>*/}
                </tbody>
            </Table>
        </>
    )
}

AccountSummary.displayName = "AccountSummary";

const useRenderers = (account: Account) => {

    const getMenuItems = () => {

        if (!account) return [];

        const items: MenuItem[] = [{ route: `/accounts/${account.id}/tag-rules`, text: "Tag Rules" }];

        switch (account.controller) {
            case AccountController.Import:
                items.push({ route: `/accounts/${account.id}/import`, text: "Import" });
        }

        return items;
    };

    return {
        getMenuItems,
    };
}

export interface AccountSummaryProps {
}