import React from "react";

import { AccountList } from "../../components";
import { Page } from "@andrewmclachlan/mooapp";
import { Link } from "react-router-dom";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";

export const Accounts: React.FC = () => (
    <Page title="Accounts" breadcrumbs={[{ text: "Accounts", route: "/accounts" }]} actions={[<Link className="btn btn-primary" to="/accounts/create"><FontAwesomeIcon icon="plus" />Create Account</Link>]}>
        <AccountList />
    </Page>
);
