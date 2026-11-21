import React from "react";
import type { PropsWithChildren, ReactNode } from "react";

import type { PageProps } from "@andrewmclachlan/moo-app";
import type { NavItem } from "@andrewmclachlan/moo-ds";
import { Reports, Rules, Sliders, Transaction } from "@andrewmclachlan/moo-icons";

import type { VirtualInstrument } from "api/types.gen";
import { isVirtualInstrument } from "utils/virtualInstruments";
import { useAccount } from "./AccountProvider";
import { InstrumentPage } from "./InstrumentPage";

export const AccountPage: React.FC<PropsWithChildren<AccountPageProps>> = ({ children, ...props }) => {

    const account = useAccount();

    if (!account) return null;

    const isVirtual = isVirtualInstrument(account);
    const baseRoute = isVirtual ? `/accounts/${(account as VirtualInstrument).parentId}/virtual/${account.id}` : `/accounts/${account.id}`;

    const navItems: (NavItem | ReactNode)[] = [
        { route: `${baseRoute}/transactions`, text: "Transactions", icon: Transaction },
    ];

    if (!isVirtual) {
        navItems.push({ route: `${baseRoute}/reports`, text: "Reports", icon: Reports });
        navItems.push({ route: `${baseRoute}/rules`, text: "Rules", icon: Rules });
    }

    navItems.push({ route: `${baseRoute}/manage`, text: "Manage", icon: Sliders });

    return (
        <InstrumentPage instrument={account} instrumentRoute={baseRoute} instrumentNavItems={navItems} {...props}>
            {children}
        </InstrumentPage>
    )
}

export interface AccountPageProps extends PageProps {
}
