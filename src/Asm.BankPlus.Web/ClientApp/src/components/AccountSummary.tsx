import "./AccountSummary.scss";

import React from "react";

import { Account, AccountController } from "../models";
import { AccountBalance } from ".";
import { Link } from "react-router-dom";

export const AccountSummary: React.FC<AccountSummaryProps> = (props) => {

    const { renderMenu } = useRenderers(props);

    if (!props.account) return null;

    return (
        <section className="account-summary">
            <header>
                <h1>{props.account.name}</h1>
                <nav className="control-panel">
                    {renderMenu()}
                </nav>
            </header>

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
        </section>
    )
}

AccountSummary.displayName = "AccountSummary";

const useRenderers = (props: AccountSummaryProps) => {

    const renderMenu = () => {

        const items = [<li><Link to={`/accounts/${props.account.id}/tag-rules`}>Tag Rules</Link></li>];

        switch (props.account.controller) {
            case AccountController.Import:
                items.push(<li><Link to={`/accounts/${props.account.id}/import`}>Import</Link></li>);
        }

        return <ul>{items}</ul>;
    };

    return {
        renderMenu,
    };
}

export interface AccountSummaryProps {
    account: Account;
}