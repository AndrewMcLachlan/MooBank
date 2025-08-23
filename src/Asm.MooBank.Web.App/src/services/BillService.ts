import { useApiGet, useApiPagedGet } from "@andrewmclachlan/moo-app";
import { PagedResult } from "@andrewmclachlan/moo-ds";
import { Bill, BillAccount } from "models/bills";
import { AccountTypeSummary } from "models/bills";

const billsKey = "bills";
const summariesKey = "bills-account-summaries";
const accountsKey = "bills-accounts";

export const useBillAccountSummaries = () => useApiGet<AccountTypeSummary[]>([summariesKey], "api/bills/accounts/types");

export const useBillAccountsByType = (utilityType: string) => useApiGet<BillAccount[]>([accountsKey, utilityType], `api/bills/accounts/types/${utilityType}`);

export const useBillAccounts = () => useApiGet<BillAccount[]>([accountsKey], "api/bills/accounts");

export const useBillAccount = (id: string) => useApiGet<BillAccount>([accountsKey], `api/bills/accounts/${id}`);

export const useAllBills = (pageNumber: number, pageSize: number) => useApiPagedGet<PagedResult<Bill>>([billsKey, pageNumber, pageSize], `api/bills?pageNumber=${pageNumber}&pageSize=${pageSize}`);

export const useBills = (id: string, pageNumber: number, pageSize: number) => useApiPagedGet<PagedResult<Bill>>([billsKey, id, pageNumber, pageSize], `api/bills/accounts/${id}/bills?pageNumber=${pageNumber}&pageSize=${pageSize}`);
