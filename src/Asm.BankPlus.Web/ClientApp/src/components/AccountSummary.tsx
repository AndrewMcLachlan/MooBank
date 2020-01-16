import "./AccountSummary.scss";

import React from "react";

import { Account, AccountController } from "../models";
import { AccountBalance } from ".";
import { PageHeader } from "./PageHeader";
import { MenuItem } from "models/MenuItem";

export const AccountSummary: React.FC<AccountSummaryProps> = (props) => {

    const { getMenuItems } = useRenderers(props);

    if (!props.account) return null;

    return (
        <PageHeader title={props.account.name} menuItems={getMenuItems()}>
            <table className="table">
                <tbody>
                    <tr>
                        <th>Balance</th>
                        <td><AccountBalance balance={props.account.currentBalance} /></td>
                    </tr>
                    <tr>
                        <th>Available Balance</th>
                        <td><AccountBalance balance={props.account.availableBalance} /></td>
                    </tr>
                </tbody>
            </table>
        </PageHeader>
    )
}

AccountSummary.displayName = "AccountSummary";

const useRenderers = (props: AccountSummaryProps) => {


    const getMenuItems = () => {

        const items: MenuItem[] = [{ route: `/accounts/${props.account.id}/tag-rules`, text: "Tag Rules" }];

        switch (props.account.controller) {
            case AccountController.Import:
                items.push({ route: `/accounts/${props.account.id}/import`, text: "Import" });
        }

        return items;
    };

    return {
        getMenuItems,
    };
}

export interface AccountSummaryProps {
    account: Account;
}