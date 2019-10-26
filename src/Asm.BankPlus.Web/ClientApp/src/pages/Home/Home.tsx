import React from "react";

import {  AccountList } from "../../components";
import { usePageTitle } from "hooks";

export const Home: React.FC = (props) => {

    usePageTitle("Home");

    return <AccountList />
 
}
