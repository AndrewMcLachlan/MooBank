import React from "react";

import { Section } from "@andrewmclachlan/moo-ds";
import classNames from "classnames";
import { format } from "date-fns/format";
import { parseISO } from "date-fns/parseISO";
import { InstitutionAccount } from "models";
import { useInstitutions } from "services";
import { useAccount } from "./AccountProvider";
import { Amount } from "./Amount";
import { KeyValue } from "./KeyValue";

export const AccountSummary: React.FC<AccountSummaryProps> = ({ className, ...props }) => {

    const account = useAccount();
    const { data: institutions } = useInstitutions();

    if (!account) return null;

    return (
        <Section className={classNames("summary", className)} {...props} header={account.name}>
            <KeyValue>
                <div>Balance</div>
                <div className="balance amount"><Amount amount={account.currentBalanceLocalCurrency} /></div>
            </KeyValue>
            <KeyValue hidden={account.currentBalance === account.currentBalanceLocalCurrency || !account.currentBalanceLocalCurrency}>
                <div>Balance ({account.currency})</div>
                <div className="balance amount"><Amount amount={account.currentBalance} /></div>
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
