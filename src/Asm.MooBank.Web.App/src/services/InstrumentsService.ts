import { AccountList } from "../models";
import { useApiGet } from "@andrewmclachlan/mooapp";
import { ListItem } from "models/ListItem";

export const formattedAccountsKey = "formatted-accounts";
export const accountListKey = "account-list";


export const useFormattedAccounts = () => useApiGet<AccountList>([formattedAccountsKey], "api/instruments/summary");

export const useAccountsList = () => useApiGet<ListItem<string>[]>([accountListKey], "api/instruments/list");
