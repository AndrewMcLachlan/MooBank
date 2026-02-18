import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
    getBillAccountSummariesByTypeOptions,
    getBillAccountSummariesByTypeQueryKey,
    getBillAccountsByTypeOptions,
    getBillAccountsOptions,
    getBillAccountsQueryKey,
    getBillAccountOptions,
    getAllBillsOptions,
    getAllBillsQueryKey,
    getBillsByUtilityTypeOptions,
    getBillsForAnAccountOptions,
    getBillsForAnAccountQueryKey,
    createBillAccountMutation,
    createBillMutation,
    getCostPerUnitReportOptions,
    getServiceChargeReportOptions,
    getUsageReportOptions,
} from "api/@tanstack/react-query.gen";
import {
    Create,
    UtilityType,
} from "api/types.gen";
import { BillAccount, CreateBillAccount, CreateBill, Bill } from "models/bills";
import { toast } from "react-toastify";

// Re-export generated types that consumers import from this file
export type { CostPerUnitReport, CostDataPoint, ServiceChargeReport, ServiceChargeDataPoint, UsageReport, UsageDataPoint } from "api/types.gen";

export interface BillFilter {
    startDate?: string;
    endDate?: string;
    accountId?: string;
    utilityType?: string;
}

export const useBillAccountSummaries = () => useQuery({ ...getBillAccountSummariesByTypeOptions() });

export const useBillAccountsByType = (utilityType: string) => useQuery({
    ...getBillAccountsByTypeOptions({ path: { type: utilityType as UtilityType } }),
    select: (data) => data as unknown as BillAccount[],
});

export const useBillAccounts = () => useQuery({
    ...getBillAccountsOptions(),
    select: (data) => data as unknown as BillAccount[],
});

export const useBillAccount = (id: string) => useQuery({
    ...getBillAccountOptions({ path: { instrumentId: id } }),
    select: (data) => data as unknown as BillAccount,
});

export const useCreateBillAccount = () => {
    const queryClient = useQueryClient();

    const { mutateAsync, ...rest } = useMutation({
        ...createBillAccountMutation(),
        onSettled: () => {
            queryClient.invalidateQueries({ queryKey: getBillAccountsQueryKey() });
            queryClient.invalidateQueries({ queryKey: getBillAccountSummariesByTypeQueryKey() });
        },
    });

    return {
        mutateAsync: (account: CreateBillAccount) =>
            toast.promise(mutateAsync({ body: account as unknown as Create }), { pending: "Creating account", success: "Account created", error: "Failed to create account" }),
        ...rest,
    };
};

export const useAllBills = (pageNumber: number, pageSize: number, filter?: BillFilter) =>
    useQuery({
        ...getAllBillsOptions({
            query: {
                PageNumber: pageNumber,
                PageSize: pageSize,
                StartDate: filter?.startDate,
                EndDate: filter?.endDate,
                AccountId: filter?.accountId,
                UtilityType: filter?.utilityType as UtilityType | undefined,
            },
        }),
        select: (data) => data as unknown as { results: Bill[], total: number },
    });

export const useBills = (id: string, pageNumber: number, pageSize: number) => useQuery({
    ...getBillsForAnAccountOptions({
        path: { instrumentId: id },
        query: { PageNumber: pageNumber, PageSize: pageSize },
    }),
    select: (data) => data as unknown as { results: Bill[], total: number },
});

export const useBillsByUtilityType = (utilityType: string, pageNumber: number, pageSize: number, filter?: BillFilter) => useQuery({
    ...getBillsByUtilityTypeOptions({
        path: { utilityType: utilityType as UtilityType },
        query: {
            PageNumber: pageNumber,
            PageSize: pageSize,
            StartDate: filter?.startDate,
            EndDate: filter?.endDate,
            AccountId: filter?.accountId,
        },
    }),
    enabled: !!utilityType,
    select: (data) => data as unknown as { results: Bill[], total: number },
});

export const useCostPerUnitReport = (start: string, end: string, accountId?: string, utilityType?: string) => useQuery({
    ...getCostPerUnitReportOptions({
        query: {
            Start: start,
            End: end,
            AccountId: accountId,
            UtilityType: utilityType as UtilityType | undefined,
        },
    }),
    enabled: !!start && !!end,
});

export const useServiceChargeReport = (start: string, end: string, accountId?: string, utilityType?: string) => useQuery({
    ...getServiceChargeReportOptions({
        query: {
            Start: start,
            End: end,
            AccountId: accountId,
            UtilityType: utilityType as UtilityType | undefined,
        },
    }),
    enabled: !!start && !!end,
});

export const useUsageReport = (start: string, end: string, accountId?: string, utilityType?: string) => useQuery({
    ...getUsageReportOptions({
        query: {
            Start: start,
            End: end,
            AccountId: accountId,
            UtilityType: utilityType as UtilityType | undefined,
        },
    }),
    enabled: !!start && !!end,
});

export const useCreateBill = () => {
    const queryClient = useQueryClient();

    const { mutateAsync, ...rest } = useMutation({
        ...createBillMutation(),
        onSettled: (_data, _error, variables) => {
            queryClient.invalidateQueries({ queryKey: getAllBillsQueryKey({ query: {} as any }) });
            queryClient.invalidateQueries({ queryKey: getBillsForAnAccountQueryKey({ path: { instrumentId: variables.path!.instrumentId } }) });
            queryClient.invalidateQueries({ queryKey: getBillAccountsQueryKey() });
        },
    });

    return {
        mutateAsync: (accountId: string, bill: CreateBill) =>
            toast.promise(mutateAsync({ body: bill as unknown as Create, path: { instrumentId: accountId } }), { pending: "Creating bill", success: "Bill created", error: "Failed to create bill" }),
        ...rest,
    };
};
