import React from "react";
import { useQuery } from "react-query";

import { Account, Accounts } from "../models";
import { httpClient } from "./HttpClient";

export const useAccounts = () => {

    const url = `api/accounts`;

    return useQuery(["accounts"], async () : Promise<Accounts> => (await httpClient.get(url)).data);
}

export const useAccount = (accountId: string) => {
    const url = `api/accounts/${accountId}`;

    return useQuery(["account", accountId], async () : Promise<Account> => (await httpClient.get(url)).data);
}