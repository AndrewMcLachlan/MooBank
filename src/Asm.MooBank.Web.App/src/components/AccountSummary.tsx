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
        <Section className={classNames("summary", className)} {...props} title={account.name}>
            <KeyValue>
                <div>Balance</div>
                <div className="balance amount"><AccountBalance balance={account.currentBalance} /></div>
            </KeyValue>
            <KeyValue>
                <div>Last Transaction</div>
                <div>{account.lastTransaction ? format(parseISO(account.lastTransaction), "dd/MM/yyyy") : "-"}</div>
            </KeyValue>
            <hr />
            <KeyValue>
                <div>Type</div>
                <div>{(account as InstitutionAccount).accountType ?? "Virtual"}</div>
            </KeyValue>
            <KeyValue hidden={!(account as InstitutionAccount).institutionId}>
                <div>Institution</div>
                <div>{institutions?.find(i => i.id === (account as InstitutionAccount).institutionId)?.name}</div>
            </KeyValue>
        </Section>
    )
}

AccountSummary.displayName = "AccountSummary";

export interface AccountSummaryProps extends React.HTMLAttributes<HTMLElement> {
}