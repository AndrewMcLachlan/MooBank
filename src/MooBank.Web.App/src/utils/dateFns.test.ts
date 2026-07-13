import { describe, it, expect } from "vitest";
import {
    formatISODate,
    formatDisplayDate,
    formatDateShort,
    formatDateRange,
    isMonthSelected,
    numberOfMonths,
    subtractYear,
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
