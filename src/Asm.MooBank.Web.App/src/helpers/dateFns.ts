import { addMonths } from "date-fns/addMonths";
import { addYears } from "date-fns/addYears";
import { endOfMonth } from "date-fns/endOfMonth";
import { endOfYear } from "date-fns/endOfYear";
import { format } from "date-fns/format";
import { parseISO } from "date-fns/parseISO";
import { startOfMonth } from "date-fns/startOfMonth";
import { startOfYear } from "date-fns/startOfYear";

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
export const thisYear: Period = { startDate: startOfYear(new Date()), endDate: lastMonth.endDate };
export const lastYear: Period = { startDate: startOfYear(addYears(new Date(), -1)), endDate: endOfYear(addYears(new Date(), -1)) };
export const allTime: Period = { startDate: startOfYear(addYears(new Date(), -50)), endDate: endOfYear(new Date()) };

export const formatISODate = (date: Date) => format(date, "yyyy-MM-dd");

export const formatDate = (date?: string) => date ? format(parseISO(date), "dd/MM/yyyy") : "-";

export const isMonthSelected = (months: number, month: number) => (months & (1 << month)) !== 0;

export const numberOfMonths = (months: number) => {
    let count = 0;
    for (let i = 0; i < 12; i++) {
        if (isMonthSelected(months, i)) {
            count++;
        }
    }
    return count;
}

export const subtractYear = (period: Period) => ({ startDate: addYears(period.startDate, -1), endDate: addYears(period.endDate, -1) });

export const lastMonthName = format(lastMonth.startDate, 'MMMM');
