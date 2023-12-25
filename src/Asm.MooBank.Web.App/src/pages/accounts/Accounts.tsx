import React from "react";

import { AccountList } from "../../components";
import { Page } from "@andrewmclachlan/mooapp";
import { Link } from "react-router-dom";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";

export const Accounts: React.FC = () => (
    <Page title="Accounts" breadcrumbs={[{ text: "Accounts", route: "/accounts" }]} actions={
        [<Link key="create-account" className="btn btn-primary" to="/accounts/create"><FontAwesomeIcon icon="plus" />Add Account</Link>,
        <Link key="crate-stock" className="btn btn-primary" to="/stock/create"><FontAwesomeIcon icon="plus" />Add Stock Holding</Link>
        ]}>
        <AccountList />
    </Page>
);
