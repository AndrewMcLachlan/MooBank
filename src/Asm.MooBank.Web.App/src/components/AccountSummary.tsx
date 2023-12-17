import React from "react";

import { AccountBalance } from ".";
import { useAccount } from "./AccountProvider";
import { Section } from "@andrewmclachlan/mooapp";
import classNames from "classnames";
import format from "date-fns/format";
import parseISO from "date-fns/parseISO";
import { useInstitutions } from "services";

export const AccountSummary: React.FC<AccountSummaryProps> = ({className, ...props }) => {

    const account = useAccount();
    const { data: institutions } = useInstitutions();

    if (!account) return null;

    return (
        <Section className={classNames("account-summary", className)} {...props} title={account.name}>
            <div className="key-value">
                <div>Balance</div>
                <div className="balance amount"><AccountBalance balance={account.currentBalance} /></div>
            </div>
            <div className="key-value">
                <div>Last Transaction</div>
                <div>{account.lastTransaction ? format(parseISO(account.lastTransaction), "dd/MM/yyyy") : "-"}</div>
            </div><hr/>
            <div className="key-value">
                <div>Type</div>
                <div>{account.accountType ?? "Virtual"}</div>
            </div>
            <div className="key-value" hidden={!account.institutionId}>
                <div>Institution</div>
                <div>{institutions?.find(i => i.id === account.institutionId)?.name}</div>
            </div>

        </Section>
    )
}

AccountSummary.displayName = "AccountSummary";

export interface AccountSummaryProps extends React.HTMLAttributes<HTMLElement> {
}