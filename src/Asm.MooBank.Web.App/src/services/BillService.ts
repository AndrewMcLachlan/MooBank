import { useApiGet, useApiPagedGet, useApiPost } from "@andrewmclachlan/moo-app";
import { PagedResult } from "@andrewmclachlan/moo-ds";
import { useQueryClient } from "@tanstack/react-query";
import { AccountTypeSummary, Bill, BillAccount, CreateBill, CreateBillAccount } from "models/bills";
import { toast } from "react-toastify";

const billsKey = "bills";
const summariesKey = "bills-account-summaries";
const accountsKey = "bills-accounts";

export const useBillAccountSummaries = () => useApiGet<AccountTypeSummary[]>([summariesKey], "api/bills/accounts/types");

export const useBillAccountsByType = (utilityType: string) => useApiGet<BillAccount[]>([accountsKey, utilityType], `api/bills/accounts/types/${utilityType}`);

export const useBillAccounts = () => useApiGet<BillAccount[]>([accountsKey], "api/bills/accounts");

export const useBillAccount = (id: string) => useApiGet<BillAccount>([accountsKey, id], `api/bills/accounts/${id}`);

export const useCreateBillAccount = () => {
    const queryClient = useQueryClient();

    const { mutateAsync, ...rest } = useApiPost<BillAccount, null, CreateBillAccount>(() => `api/bills/accounts`, {
        onSettled: () => {
            queryClient.invalidateQueries({ queryKey: [accountsKey] });
            queryClient.invalidateQueries({ queryKey: [summariesKey] });
        }
    });

    return {
        mutateAsync: (account: CreateBillAccount) =>
            toast.promise(mutateAsync([null, account]), { pending: "Creating account", success: "Account created", error: "Failed to create account" }),
        ...rest,
    };
};

export interface BillFilter {
    startDate?: string;
    endDate?: string;
    accountId?: string;
    utilityType?: string;
}

const buildFilterQueryString = (filter?: BillFilter): string => {
    if (!filter) return "";
    const params = new URLSearchParams();
    if (filter.startDate) params.append("startDate", filter.startDate);
    if (filter.endDate) params.append("endDate", filter.endDate);
    if (filter.accountId) params.append("accountId", filter.accountId);
    if (filter.utilityType) params.append("utilityType", filter.utilityType);
    const queryString = params.toString();
    return queryString ? `&${queryString}` : "";
};

export const useAllBills = (pageNumber: number, pageSize: number, filter?: BillFilter) =>
    useApiPagedGet<PagedResult<Bill>>(
        [billsKey, pageNumber, pageSize, filter],
        `api/bills?pageNumber=${pageNumber}&pageSize=${pageSize}${buildFilterQueryString(filter)}`
    );

export const useBills = (id: string, pageNumber: number, pageSize: number) => useApiPagedGet<PagedResult<Bill>>([billsKey, id, pageNumber, pageSize], `api/bills/accounts/${id}/bills?pageNumber=${pageNumber}&pageSize=${pageSize}`);

export const useBillsByUtilityType = (utilityType: string, pageNumber: number, pageSize: number, filter?: BillFilter) => {
    const filterParams = buildFilterQueryString(filter);
    return useApiPagedGet<PagedResult<Bill>>(
        [billsKey, "byType", utilityType, pageNumber, pageSize, filter],
        `api/bills/types/${utilityType}/bills?pageNumber=${pageNumber}&pageSize=${pageSize}${filterParams}`,
        { enabled: !!utilityType }
    );
};

export interface CostPerUnitReport {
    start: string;
    end: string;
    dataPoints: CostDataPoint[];
}

export interface CostDataPoint {
    date: string;
    accountName: string;
    averagePricePerUnit: number;
    totalUsage: number;
}

export interface ServiceChargeReport {
    start: string;
    end: string;
    dataPoints: ServiceChargeDataPoint[];
}

export interface ServiceChargeDataPoint {
    date: string;
    accountName: string;
    averageChargePerDay: number;
}

export interface UsageReport {
    start: string;
    end: string;
    dataPoints: UsageDataPoint[];
}

export interface UsageDataPoint {
    date: string;
    accountName: string;
    usagePerDay: number;
}

export const useCostPerUnitReport = (start: string, end: string, accountId?: string, utilityType?: string) => {
    const params = new URLSearchParams();
    params.append("start", start);
    params.append("end", end);
    if (accountId) params.append("accountId", accountId);
    if (utilityType) params.append("utilityType", utilityType);

    return useApiGet<CostPerUnitReport>(
        [billsKey, "reports", "cost-per-unit", start, end, accountId, utilityType],
        `api/bills/reports/cost-per-unit?${params.toString()}`,
        { enabled: !!start && !!end }
    );
};

export const useServiceChargeReport = (start: string, end: string, accountId?: string, utilityType?: string) => {
    const params = new URLSearchParams();
    params.append("start", start);
    params.append("end", end);
    if (accountId) params.append("accountId", accountId);
    if (utilityType) params.append("utilityType", utilityType);

    return useApiGet<ServiceChargeReport>(
        [billsKey, "reports", "service-charge", start, end, accountId, utilityType],
        `api/bills/reports/service-charge?${params.toString()}`,
        { enabled: !!start && !!end }
    );
};

export const useUsageReport = (start: string, end: string, accountId?: string, utilityType?: string) => {
    const params = new URLSearchParams();
    params.append("start", start);
    params.append("end", end);
    if (accountId) params.append("accountId", accountId);
    if (utilityType) params.append("utilityType", utilityType);

    return useApiGet<UsageReport>(
        [billsKey, "reports", "usage", start, end, accountId, utilityType],
        `api/bills/reports/usage?${params.toString()}`,
        { enabled: !!start && !!end }
    );
};

export const useCreateBill = () => {
    const queryClient = useQueryClient();

    const { mutateAsync, ...rest } = useApiPost<Bill, string, CreateBill>((accountId) => `api/bills/accounts/${accountId}/bills`, {
        onSettled: (_data, _error, [accountId]) => {
            queryClient.invalidateQueries({ queryKey: [billsKey] });
            queryClient.invalidateQueries({ queryKey: [billsKey, accountId] });
            queryClient.invalidateQueries({ queryKey: [accountsKey] });
        }
    });

    return {
        mutateAsync: (accountId: string, bill: CreateBill) =>
            toast.promise(mutateAsync([accountId, bill]), { pending: "Creating bill", success: "Bill created", error: "Failed to create bill" }),
        ...rest,
    };
};
