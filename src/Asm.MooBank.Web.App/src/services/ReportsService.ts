import { useQuery } from "@tanstack/react-query";
import {
    inOutReportOptions,
    inOutAverageReportOptions,
    inOutTrendReportOptions,
    tagBreakdownReportOptions,
    byTagReportOptions,
    tagTrendReportOptions,
    allTagAverageReportOptions,
    monthlyBalancesReportOptions,
    groupMonthlyBalancesReportOptions,
} from "api/@tanstack/react-query.gen";
import { formatISODate } from "../helpers/dateFns";
import { TrendReportSettings, defaultSettings, reportInterval } from "../helpers/reports";
import { transactionTypeFilter } from "store/state";

export const useInOutReport = (accountId: string, start: Date, end: Date) =>
    useQuery({
        ...inOutReportOptions({ path: { accountId, start: formatISODate(start), end: formatISODate(end) } }),
        enabled: !!start && !!end,
    });

export const useInOutAverageReport = (accountId: string, start: Date, end: Date, interval: reportInterval = "Monthly") =>
    useQuery({
        ...inOutAverageReportOptions({ path: { accountId, start: formatISODate(start), end: formatISODate(end) }, query: { Interval: interval } }),
        enabled: !!start && !!end,
    });

export const useInOutTrendReport = (accountId: string, start: Date, end: Date, settings: TrendReportSettings = defaultSettings) =>
    useQuery({
        ...inOutTrendReportOptions({ path: { accountId, start: formatISODate(start), end: formatISODate(end) }, query: { Interval: settings.interval } }),
        enabled: !!start && !!end,
    });

export const useBreakdownReport = (accountId: string, start: Date, end: Date, reportType: transactionTypeFilter, tagId?: number) =>
    useQuery({
        ...tagBreakdownReportOptions({ path: { accountId, start: formatISODate(start), end: formatISODate(end), reportType: reportType.toLowerCase(), parentTagId: tagId ?? 0 } }),
        enabled: !!start && !!end,
    });

export const useByTagReport = (accountId: string, start: Date, end: Date, reportType: transactionTypeFilter) =>
    useQuery({
        ...byTagReportOptions({ path: { accountId, start: formatISODate(start), end: formatISODate(end), reportType: reportType.toLowerCase(), parentTagId: 0 } }),
        enabled: !!start && !!end,
    });

export const useTagTrendReport = (accountId: string, start: Date, end: Date, reportType: transactionTypeFilter, tagId: number, settings: TrendReportSettings) =>
    useQuery({
        ...tagTrendReportOptions({ path: { accountId, start: formatISODate(start), end: formatISODate(end), reportType: reportType.toLowerCase(), tagId }, query: { ApplySmoothing: settings.applySmoothing } }),
    });

export const useAllTagAverageReport = (accountId: string, start: Date, end: Date, reportType: transactionTypeFilter, top: number = 20, interval: reportInterval = "Monthly") =>
    useQuery({
        ...allTagAverageReportOptions({ path: { accountId, start: formatISODate(start), end: formatISODate(end), reportType: reportType.toLowerCase() }, query: { Top: top, Interval: interval } }),
        enabled: !!start && !!end,
    });

export const useMonthlyBalancesReport = (accountId: string, start: Date, end: Date) =>
    useQuery({
        ...monthlyBalancesReportOptions({ path: { accountId, start: formatISODate(start), end: formatISODate(end) } }),
        enabled: !!start && !!end,
    });

export const useGroupMonthlyBalancesReport = (groupId: string, start: Date, end: Date) =>
    useQuery({
        ...groupMonthlyBalancesReportOptions({ path: { groupId, start: formatISODate(start), end: formatISODate(end) } }),
        enabled: !!groupId && !!start && !!end,
    });
