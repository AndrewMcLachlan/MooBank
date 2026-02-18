import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
    getBillAccountSummariesByTypeOptions,
    getBillAccountSummariesByTypeQueryKey,
    getBillAccountsByTypeOptions,
    getBillAccountsOptions,
    getBillAccountsQueryKey,
    getBillAccountOptions,
    getAllBillsQueryKey,
    getBillsForAnAccountQueryKey,
    getBillsByUtilityTypeQueryKey,
    createBillAccountMutation,
    createBillMutation,
    getCostPerUnitReportOptions,
    getServiceChargeReportOptions,
    getUsageReportOptions,
} from "api/@tanstack/react-query.gen";
import { getAllBills, getBillsForAnAccount, getBillsByUtilityType } from "api/sdk.gen";
import type {
    UtilityType,
    Bill,
    CreateBillAccount,
} from "api/types.gen";
import { PagedResult } from "@andrewmclachlan/moo-ds";
import type { CreateBill } from "helpers/bills";
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
});

export const useBillAccounts = () => useQuery({
    ...getBillAccountsOptions(),
});

export const useBillAccount = (id: string) => useQuery({
    ...getBillAccountOptions({ path: { instrumentId: id } }),
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
            toast.promise(mutateAsync({ body: account }), { pending: "Creating account", success: "Account created", error: "Failed to create account" }),
        ...rest,
    };
};

export const useAllBills = (pageNumber: number, pageSize: number, filter?: BillFilter) =>
    useQuery({
        queryKey: getAllBillsQueryKey({
            query: {
                PageNumber: pageNumber,
                PageSize: pageSize,
                StartDate: filter?.startDate,
                EndDate: filter?.endDate,
                AccountId: filter?.accountId,
                UtilityType: filter?.utilityType as UtilityType | undefined,
            },
        }),
        queryFn: async ({ signal }) => {
            const { data, headers } = await getAllBills({
                query: {
                    PageNumber: pageNumber,
                    PageSize: pageSize,
                    StartDate: filter?.startDate,
                    EndDate: filter?.endDate,
                    AccountId: filter?.accountId,
                    UtilityType: filter?.utilityType as UtilityType | undefined,
                },
                signal,
                throwOnError: true,
            });
            return { results: data, total: Number(headers['x-total-count'] ?? 0) } as PagedResult<Bill>;
        },
    });

export const useBills = (id: string, pageNumber: number, pageSize: number) => useQuery({
    queryKey: getBillsForAnAccountQueryKey({
        path: { instrumentId: id },
        query: { PageNumber: pageNumber, PageSize: pageSize },
    }),
    queryFn: async ({ signal }) => {
        const { data, headers } = await getBillsForAnAccount({
            path: { instrumentId: id },
            query: { PageNumber: pageNumber, PageSize: pageSize },
            signal,
            throwOnError: true,
        });
        return { results: data, total: Number(headers['x-total-count'] ?? 0) } as PagedResult<Bill>;
    },
});

export const useBillsByUtilityType = (utilityType: string, pageNumber: number, pageSize: number, filter?: BillFilter) => useQuery({
    queryKey: getBillsByUtilityTypeQueryKey({
        path: { utilityType: utilityType as UtilityType },
        query: {
            PageNumber: pageNumber,
            PageSize: pageSize,
            StartDate: filter?.startDate,
            EndDate: filter?.endDate,
            AccountId: filter?.accountId,
        },
    }),
    queryFn: async ({ signal }) => {
        const { data, headers } = await getBillsByUtilityType({
            path: { utilityType: utilityType as UtilityType },
            query: {
                PageNumber: pageNumber,
                PageSize: pageSize,
                StartDate: filter?.startDate,
                EndDate: filter?.endDate,
                AccountId: filter?.accountId,
            },
            signal,
            throwOnError: true,
        });
        return { results: data, total: Number(headers['x-total-count'] ?? 0) } as PagedResult<Bill>;
    },
    enabled: !!utilityType,
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
            queryClient.invalidateQueries({ queryKey: getBillsForAnAccountQueryKey({ path: { instrumentId: (variables as any).path!.instrumentId } }) });
            queryClient.invalidateQueries({ queryKey: getBillAccountsQueryKey() });
        },
    });

    return {
        mutateAsync: (accountId: string, bill: CreateBill) =>
            toast.promise(mutateAsync({ body: bill as any, path: { instrumentId: accountId } } as any), { pending: "Creating bill", success: "Bill created", error: "Failed to create bill" }),
        ...rest,
    };
};
