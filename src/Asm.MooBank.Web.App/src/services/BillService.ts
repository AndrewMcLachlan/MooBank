import { PagedResult, useApiGet, useApiPagedGet } from "@andrewmclachlan/mooapp";
import { Bill, BillAccount } from "models/bills";
import { AccountTypeSummary } from "models/bills";

const billsKey = "bills";
const summariesKey = "bills-account-summaries";
const accountsKey = "bills-accounts";

export const useBillAccountSummaries = () => useApiGet<AccountTypeSummary[]>([summariesKey], "api/bills/accounts/types");

export const useBillAccountsByType = (utilityType: string) => useApiGet<BillAccount[]>([accountsKey, utilityType], `api/bills/accounts/types/${utilityType}`);

export const useBillAccounts = () => useApiGet<BillAccount[]>([accountsKey], "api/bills/accounts");

export const useBills = (pageNumber: number, pageSize: number) => useApiPagedGet<PagedResult<Bill>>([billsKey, pageNumber, pageSize], `api/bills?pageNumber=${pageNumber}&pageSize=${pageSize}`);
