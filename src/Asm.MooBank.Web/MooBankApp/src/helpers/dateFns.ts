import addMonths from "date-fns/addMonths";
import addYears from "date-fns/addYears";
import endOfMonth from "date-fns/endOfMonth";
import endOfYear from "date-fns/endOfYear";
import format from "date-fns/format";
import parseISO from "date-fns/parseISO";
import startOfMonth from "date-fns/startOfMonth";
import startOfYear from "date-fns/startOfYear";

export interface Period {
    startDate: Date;
    endDate: Date;
}

export const periodEquals = (a?: Period, b?: Period) => a?.startDate?.getTime() === b?.startDate?.getTime() && a?.endDate?.getTime() === b?.endDate?.getTime();

export const startOfLastMonth = () => startOfMonth(addMonths(new Date(), -1));
export const endOfLastMonth = () => endOfMonth(addMonths(new Date(), -1));

export const thisMonth: Period = { startDate: startOfMonth(new Date()), endDate: endOfMonth(new Date()) };
export const lastMonth: Period = { startDate: startOfLastMonth(), endDate: endOfLastMonth() };
export const previousMonth: Period = { startDate: startOfMonth(addMonths(new Date(), -2)), endDate: endOfMonth(addMonths(new Date(), -2)) };
export const last3Months: Period = { startDate: startOfMonth(addMonths(new Date(), -3)), endDate: endOfMonth(addMonths(new Date(), -1)) };
export const last6Months: Period = { startDate: startOfMonth(addMonths(new Date(), -6)), endDate: endOfMonth(addMonths(new Date(), -1)) };
export const last12Months: Period = { startDate: startOfMonth(addMonths(new Date(), -12)), endDate: endOfMonth(addMonths(new Date(), -1)) };
export const lastYear: Period = { startDate: startOfYear(addYears(new Date(), -1)), endDate: endOfYear(addYears(new Date(), -1)) };
export const allTime: Period = { startDate: startOfYear(addYears(new Date(), -50)), endDate: endOfYear(new Date()) };

export const formatISODate = (date: Date) => format(date, "yyyy-MM-dd");