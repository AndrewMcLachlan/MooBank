import { Account, Accounts } from "../models";
import { useApiQuery } from "./useApiQuery";

export const useAccounts = () => useApiQuery<Accounts>(["accounts"], `api/accounts`);

export const useAccount = (accountId: string) => useApiQuery<Account>(["account", accountId], `api/accounts/${accountId}`);