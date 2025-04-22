import React from "react";

import { AccountBalance } from ".";
import { useAccount } from "./AccountProvider";
import { Section } from "@andrewmclachlan/mooapp";
import classNames from "classnames";
import { format } from "date-fns/format";
import { parseISO } from "date-fns/parseISO";
import { useInstitutions } from "services";
import { KeyValue } from "./KeyValue";
import { InstitutionAccount } from "models";

export const AccountSummary: React.FC<AccountSummaryProps> = ({ className, ...props }) => {

    const account = useAccount();
    const { data: institutions } = useInstitutions();

    if (!account) return null;

    return (
        <Section className={classNames("summary", className)} {...props} header={account.name}>
            <KeyValue>
                <div>Balance</div>
                <div className="balance amount"><AccountBalance balance={account.currentBalanceLocalCurrency} /></div>
            </KeyValue>
            <KeyValue hidden={account.currentBalance === account.currentBalanceLocalCurrency || !account.currentBalanceLocalCurrency}>
                <div>Balance ({account.currency})</div>
                <div className="balance amount"><AccountBalance balance={account.currentBalance} /></div>
            </KeyValue>
            <KeyValue className="d-none d-lg-flex">
                <div>Last Transaction</div>
                <div>{account.lastTransaction ? format(parseISO(account.lastTransaction), "dd/MM/yyyy") : "-"}</div>
            </KeyValue>
            <hr className="d-none d-lg-block" />
            <KeyValue className="d-none d-lg-flex">
                <div>Type</div>
                <div>{(account as InstitutionAccount).instrumentType ?? "Virtual"}</div>
            </KeyValue>
            <KeyValue hidden={!(account as InstitutionAccount).institutionId} className="d-none d-lg-flex">
                <div>Institution</div>
                <div>{institutions?.find(i => i.id === (account as InstitutionAccount).institutionId)?.name}</div>
            </KeyValue>
        </Section>
    )
}

AccountSummary.displayName = "AccountSummary";

export interface AccountSummaryProps extends React.HTMLAttributes<HTMLElement> {
}
