import { describe, it, expect } from "vitest";
import { getPeriodLabel } from "utils/periodLabel";
import { periodOptions } from "models/periodOptions";

describe("getPeriodLabel", () => {
    it("returns an empty string for an undefined filter", () => {
        expect(getPeriodLabel(undefined)).toBe("");
    });

    it("returns an empty string when the start is missing", () => {
        expect(getPeriodLabel({ end: "2026-01-31T00:00:00.000Z" })).toBe("");
    });

    it("returns an empty string when the end is missing", () => {
        expect(getPeriodLabel({ start: "2026-01-01T00:00:00.000Z" })).toBe("");
    });

    it("returns the matching period option's label when the filter matches a known option", () => {
        const option = periodOptions[0];
        const filter = {
            start: option.startDate.toISOString(),
            end: option.endDate.toISOString(),
        };

        expect(getPeriodLabel(filter)).toBe(option.label);
    });

    it("returns a formatted date range when the filter doesn't match any period option", () => {
        expect(getPeriodLabel({ start: "2020-01-01", end: "2020-01-31" })).toBe("01 Jan 2020 → 31 Jan 2020");
    });
});
