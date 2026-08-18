import { addMonths } from "date-fns/addMonths";
import { addYears } from "date-fns/addYears";
import { endOfDay } from "date-fns/endOfDay";
import { endOfMonth } from "date-fns/endOfMonth";
import { endOfYear } from "date-fns/endOfYear";
import { format } from "date-fns/format";
import { isValid } from "date-fns/isValid";
import { parse } from "date-fns/parse";
import { parseISO } from "date-fns/parseISO";
import { startOfDay } from "date-fns/startOfDay";
import { startOfMonth } from "date-fns/startOfMonth";
import { startOfYear } from "date-fns/startOfYear";
import type { Period } from "models/dateFns";

export const startOfLastMonth = () => startOfMonth(addMonths(new Date(), -1));
export const endOfLastMonth = () => endOfMonth(addMonths(new Date(), -1));

// Periods are functions (evaluated on access) rather than module-level constants,
// so long-lived tabs don't keep serving stale date ranges.
export const thisMonth = (): Period => ({ startDate: startOfMonth(new Date()), endDate: endOfMonth(new Date()) });
export const lastMonth = (): Period => ({ startDate: startOfLastMonth(), endDate: endOfLastMonth() });
export const previousMonth = (): Period => ({ startDate: startOfMonth(addMonths(new Date(), -2)), endDate: endOfMonth(addMonths(new Date(), -2)) });
export const last3Months = (): Period => ({ startDate: startOfMonth(addMonths(new Date(), -3)), endDate: endOfMonth(addMonths(new Date(), -1)) });
export const last6Months = (): Period => ({ startDate: startOfMonth(addMonths(new Date(), -6)), endDate: endOfMonth(addMonths(new Date(), -1)) });
export const last12Months = (): Period => ({ startDate: startOfMonth(addMonths(new Date(), -12)), endDate: endOfMonth(addMonths(new Date(), -1)) });
export const thisYear = (): Period => ({ startDate: startOfYear(new Date()), endDate: endOfYear(new Date()) });
export const lastYear = (): Period => ({ startDate: startOfYear(addYears(new Date(), -1)), endDate: endOfYear(addYears(new Date(), -1)) });
export const allTime = (): Period => ({ startDate: startOfYear(addYears(new Date(), -50)), endDate: endOfYear(new Date()) });

export const formatISODate = (date: Date) => format(date, "yyyy-MM-dd");

export const formatDisplayDate = (date?: string) => date ? format(parseISO(date), "dd/MM/yyyy") : "-";

export const formatDateShort = (date?: string) => date ? format(parseISO(date), "dd MMM yy") : "-";

export const formatDateRange = (start?: string, end?: string) => {
    if (!start || !end) return "-";
    const startDate = parseISO(start);
    const endDate = parseISO(end);
    const startFormat = startDate.getFullYear() === endDate.getFullYear() ? "dd MMM" : "dd MMM yyyy";
    return `${format(startDate, startFormat)} - ${format(endDate, "dd MMM yyyy")}`;
};

/*
 * Date-only search params. The URL carries a range as two local calendar dates (yyyy-MM-dd) -- the
 * time of day is never something the user chose, and a full ISO instant in the query string reads
 * as the wrong day once UTC has shifted it. The API still filters on instants (TransactionTime <=
 * End), so the range is expanded back to the edges of those days on the way to the query.
 */
export const isDateParam = (value: unknown): value is string => typeof value === "string" && /^\d{4}-\d{2}-\d{2}$/.test(value);

/**
 * Normalises a date value from the URL. Full ISO instants (the shape older links carry) are
 * converted to the local day they fall on rather than truncated, so a bookmarked range still
 * covers the months it did before. Returns undefined for anything unparseable.
 */
export const toDateParam = (value: string): string | undefined => {
    if (isDateParam(value)) return value;

    const parsed = parseISO(value);
    return isValid(parsed) ? formatISODate(parsed) : undefined;
};

/** Expands a yyyy-MM-dd param to the first instant of that day, as the API expects it. */
export const startOfDayISO = (date: string) => startOfDay(parseISO(date)).toISOString();

/** Expands a yyyy-MM-dd param to the last instant of that day, so the end date is inclusive. */
export const endOfDayISO = (date: string) => endOfDay(parseISO(date)).toISOString();

/*
 * Month-granularity ranges. The date range selector asks "which months?" rather than "which days?",
 * so a custom range is stored as two "yyyy-MM" strings and only resolved to days on the way out.
 * The format sorts lexicographically, which is what lets the callers below order a pair by
 * comparing the strings directly.
 */
export const monthFormat = "yyyy-MM";

export const formatMonth = (date: Date) => format(date, monthFormat);

export const parseMonth = (month: string) => parse(month, monthFormat, new Date());

export const isMonth = (value: unknown): value is string => typeof value === "string" && /^\d{4}-(0[1-9]|1[0-2])$/.test(value);

const orderMonths = (a: string, b: string) => a <= b ? [a, b] : [b, a];

export const monthRange = (startMonth: string, endMonth: string): Period => {
    const [from, to] = orderMonths(startMonth, endMonth);
    return { startDate: startOfMonth(parseMonth(from)), endDate: endOfMonth(parseMonth(to)) };
};

/** Compact label for a month range, e.g. "Mar 2025", "Mar – Jun 2025", "Nov 2024 – Jun 2025". */
export const formatMonthRange = (startMonth: string, endMonth: string) => {
    const [from, to] = orderMonths(startMonth, endMonth);
    const start = parseMonth(from);
    const end = parseMonth(to);

    if (from === to) return format(start, "MMM yyyy");
    if (start.getFullYear() === end.getFullYear()) return `${format(start, "MMM")} – ${format(end, "MMM yyyy")}`;
    return `${format(start, "MMM yyyy")} – ${format(end, "MMM yyyy")}`;
};

/** The days a period actually resolves to, e.g. "1 Mar 2025 – 30 Jun 2025". */
export const formatPeriod = (period: Period) =>
    `${format(period.startDate, "d MMM yyyy")} – ${format(period.endDate, "d MMM yyyy")}`;

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

export const lastMonthName = () => format(startOfLastMonth(), 'MMMM');
