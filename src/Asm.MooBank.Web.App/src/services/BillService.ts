import { PagedResult, useApiGet, useApiPagedGet } from "@andrewmclachlan/mooapp";
import { Bill } from "models/bills";

const billsKey = "bills";

export const useBills = (pageNumber: number, pageSize: number) => useApiPagedGet<PagedResult<Bill>>([billsKey, pageNumber, pageSize], `api/bills?pageNumber=${pageNumber}&pageSize=${pageSize}`);
