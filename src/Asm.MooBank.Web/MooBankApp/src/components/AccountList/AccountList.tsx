import "./AccountList.scss";

import React from "react";
import { Spinner, Table } from "react-bootstrap";

import { AccountRow } from "./AccountRow";
import { getBalanceString } from "../../helpers";
import { useFormattedAccounts } from "../../services";
import { AccountController } from "../../models";
import { ManualAccountRow } from "./ManualAccountRow";
import { AccountListGroup } from "./AccountListGroup";

export const AccountList: React.FC<AccountListProps> = () => {

    const { data, isLoading } = useFormattedAccounts();

    return (
        data?.accountGroups.map((ag, index) => 
        <AccountListGroup accountGroup={ag} isLoading={isLoading} key={index} />
        )
    );
}

AccountList.displayName = "AccountList";

export interface AccountListProps {

}