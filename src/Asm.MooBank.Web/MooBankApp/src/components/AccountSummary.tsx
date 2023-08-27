import React from "react";

import { AccountBalance } from ".";
import { useAccount } from "./AccountProvider";
import { Section } from "@andrewmclachlan/mooapp";
import classNames from "classnames";
import format from "date-fns/format";
import parseISO from "date-fns/parseISO";
import { AccountType } from "models";

export const AccountSummary: React.FC<AccountSummaryProps> = ({className, ...props }) => {

    const account = useAccount();

    if (!account) return null;

    return (
        <Section className={classNames("account-summary", className)} {...props} title={account.name}>
            <div className="key-value">
                <div>Balance</div>
                <div className="balance amount"><AccountBalance balance={account.currentBalance} /></div>
            </div>
            <div className="key-value">
                <div>Last Transaction</div>
                <div>{format(parseISO(account.lastTransaction), "dd/MM/yyyy")}</div>
            </div><hr/>
            <div className="key-value">
                <div>Type</div>
                <div>{AccountType[account.accountType]}</div>
            </div>

        </Section>
    )
}

AccountSummary.displayName = "AccountSummary";

export interface AccountSummaryProps extends React.HTMLAttributes<HTMLElement> {
}