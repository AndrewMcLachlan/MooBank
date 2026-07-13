import { describe, it, expect } from "vitest";
import { valueAsNumber } from "utils/valueAsNumber";

describe("valueAsNumber", () => {
    it("returns the default value when value is undefined", () => {
        expect(valueAsNumber({ value: undefined, valueAsNumber: 42 }, 7)).toBe(7);
    });

    it("returns the default value when value is null", () => {
        expect(valueAsNumber({ value: null, valueAsNumber: 42 }, 7)).toBe(7);
    });

    it("returns the default value when value is an empty string", () => {
        expect(valueAsNumber({ value: "", valueAsNumber: 42 }, 7)).toBe(7);
    });

    it("defaults to 0 when no default is provided", () => {
        expect(valueAsNumber({ value: "", valueAsNumber: 42 })).toBe(0);
    });

    it("returns valueAsNumber when value is present", () => {
        expect(valueAsNumber({ value: "123", valueAsNumber: 123 }, 7)).toBe(123);
    });

    it("returns valueAsNumber when value is 0 (falsy but not empty/null/undefined)", () => {
        expect(valueAsNumber({ value: "0", valueAsNumber: 0 }, 7)).toBe(0);
    });
});
