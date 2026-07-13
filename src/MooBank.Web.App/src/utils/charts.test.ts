import { describe, it, expect } from "vitest";
import { getStepSize } from "utils/charts";

describe("getStepSize", () => {
    it("returns undefined for an empty array", () => {
        expect(getStepSize([])).toBeUndefined();
    });

    it("returns undefined for an all-zero dataset", () => {
        expect(getStepSize([0, 0, 0])).toBeUndefined();
    });

    it("returns roughly a tenth of the rounded magnitude for a typical dataset", () => {
        // max = 90 -> magnitude = 10^floor(log10(90)) = 10
        // roundedMax = ceil(90 / 10) * 10 = 90
        // step = 90 / 10 = 9
        expect(getStepSize([0, 45, 90])).toBe(9);
    });

    it("computes the step for a dataset that isn't already a round number", () => {
        // max = 33 -> magnitude = 10^floor(log10(33)) = 10
        // roundedMax = ceil(33 / 10) * 10 = 40
        // step = 40 / 10 = 4
        expect(getStepSize([5, 12, 33])).toBe(4);
    });

    it("uses the absolute value, so negative values behave the same as positive ones", () => {
        expect(getStepSize([-90, 45, 0])).toBe(9);
    });

    it("returns undefined when the magnitude is not finite", () => {
        expect(getStepSize([Number.POSITIVE_INFINITY, 1])).toBeUndefined();
    });
});
