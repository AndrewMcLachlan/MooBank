import { describe, it, expect } from "vitest";
import {
    formatISODate,
    formatDisplayDate,
    formatDateShort,
    formatDateRange,
    isMonthSelected,
    numberOfMonths,
    subtractYear,
    isDateParam,
    toDateParam,
    startOfDayISO,
    endOfDayISO,
    monthRange,
    formatMonthRange,
    formatPeriod,
} from "utils/dateFns";

describe("formatISODate", () => {
    it("formats a Date as yyyy-MM-dd", () => {
        expect(formatISODate(new Date(2026, 0, 5))).toBe("2026-01-05");
    });
});

describe("formatDisplayDate", () => {
    it("formats an ISO date string as dd/MM/yyyy", () => {
        expect(formatDisplayDate("2026-01-05")).toBe("05/01/2026");
    });

    it("returns a dash for undefined input", () => {
        expect(formatDisplayDate(undefined)).toBe("-");
    });
});

describe("formatDateShort", () => {
    it("formats an ISO date string as dd MMM yy", () => {
        expect(formatDateShort("2026-01-05")).toBe("05 Jan 26");
    });

    it("returns a dash for undefined input", () => {
        expect(formatDateShort(undefined)).toBe("-");
    });
});

describe("formatDateRange", () => {
    it("omits the year on the start date when both dates share a year", () => {
        expect(formatDateRange("2026-01-05", "2026-01-20")).toBe("05 Jan - 20 Jan 2026");
    });

    it("includes the year on the start date when the years differ", () => {
        expect(formatDateRange("2025-12-05", "2026-01-20")).toBe("05 Dec 2025 - 20 Jan 2026");
    });

    it("returns a dash when either date is missing", () => {
        expect(formatDateRange(undefined, "2026-01-20")).toBe("-");
        expect(formatDateRange("2026-01-05", undefined)).toBe("-");
        expect(formatDateRange(undefined, undefined)).toBe("-");
    });
});

describe("isMonthSelected", () => {
    it("reads the bit for the given month from the bitmask", () => {
        const months = 0b101; // months 0 and 2 selected
        expect(isMonthSelected(months, 0)).toBe(true);
        expect(isMonthSelected(months, 1)).toBe(false);
        expect(isMonthSelected(months, 2)).toBe(true);
    });
});

describe("numberOfMonths", () => {
    it("counts the number of set bits across the 12 months", () => {
        expect(numberOfMonths(0b101)).toBe(2);
        expect(numberOfMonths(0)).toBe(0);
        expect(numberOfMonths(0xFFF)).toBe(12);
    });
});

describe("subtractYear", () => {
    it("returns a period with both dates shifted back one year", () => {
        const period = { startDate: new Date(2026, 0, 5), endDate: new Date(2026, 5, 15) };

        const result = subtractYear(period);

        expect(result.startDate.getFullYear()).toBe(2025);
        expect(result.startDate.getMonth()).toBe(0);
        expect(result.startDate.getDate()).toBe(5);
        expect(result.endDate.getFullYear()).toBe(2025);
        expect(result.endDate.getMonth()).toBe(5);
        expect(result.endDate.getDate()).toBe(15);
    });
});

describe("isDateParam", () => {
    it("accepts a yyyy-MM-dd string", () => {
        expect(isDateParam("2026-03-01")).toBe(true);
    });

    it("rejects a full ISO instant and anything that is not a date", () => {
        expect(isDateParam("2026-03-01T00:00:00.000Z")).toBe(false);
        expect(isDateParam("March")).toBe(false);
        expect(isDateParam(undefined)).toBe(false);
    });
});

describe("toDateParam", () => {
    it("passes a yyyy-MM-dd value through untouched", () => {
        expect(toDateParam("2026-03-01")).toBe("2026-03-01");
    });

    it("converts a full ISO instant to the local day it falls on, rather than truncating it", () => {
        // A link written by the old code: local midnight on 1 March, stored as UTC. Truncating the
        // string would read 28 February in any timezone ahead of UTC.
        const legacy = new Date(2026, 2, 1).toISOString();

        expect(toDateParam(legacy)).toBe("2026-03-01");
    });

    it("returns undefined for an unparseable value", () => {
        expect(toDateParam("not-a-date")).toBeUndefined();
    });
});

describe("startOfDayISO / endOfDayISO", () => {
    it("expands a date param to the instants bounding that local day", () => {
        expect(startOfDayISO("2026-03-01")).toBe(new Date(2026, 2, 1, 0, 0, 0, 0).toISOString());
        expect(endOfDayISO("2026-06-30")).toBe(new Date(2026, 5, 30, 23, 59, 59, 999).toISOString());
    });

    it("keeps the end inclusive, so a transaction later that day still matches", () => {
        const lastTransaction = new Date(2026, 5, 30, 22, 15);

        expect(new Date(endOfDayISO("2026-06-30")).getTime()).toBeGreaterThan(lastTransaction.getTime());
    });
});

describe("monthRange", () => {
    it("covers whole months, first day to last", () => {
        const period = monthRange("2026-02", "2026-04");

        expect(period.startDate).toEqual(new Date(2026, 1, 1));
        expect(period.endDate).toEqual(new Date(2026, 3, 30, 23, 59, 59, 999));
    });

    it("orders a reversed pair", () => {
        expect(monthRange("2026-04", "2026-02")).toEqual(monthRange("2026-02", "2026-04"));
    });
});

describe("formatMonthRange", () => {
    it("names a single month, a within-year range and a cross-year range", () => {
        expect(formatMonthRange("2026-03", "2026-03")).toBe("Mar 2026");
        expect(formatMonthRange("2026-03", "2026-06")).toBe("Mar – Jun 2026");
        expect(formatMonthRange("2025-11", "2026-06")).toBe("Nov 2025 – Jun 2026");
    });
});

describe("formatPeriod", () => {
    it("shows the days a period resolves to", () => {
        expect(formatPeriod(monthRange("2026-03", "2026-06"))).toBe("1 Mar 2026 – 30 Jun 2026");
    });
});
