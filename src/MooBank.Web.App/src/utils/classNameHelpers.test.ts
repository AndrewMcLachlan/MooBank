import { describe, it, expect } from "vitest";
import { numberClassName } from "utils/classNameHelpers";

describe("numberClassName", () => {
    it("returns ' negative' for negative numbers", () => {
        expect(numberClassName(-1)).toBe(" negative");
        expect(numberClassName(-0.01)).toBe(" negative");
    });

    it("returns an empty string for zero", () => {
        expect(numberClassName(0)).toBe("");
    });

    it("returns an empty string for positive numbers", () => {
        expect(numberClassName(1)).toBe("");
        expect(numberClassName(100)).toBe("");
    });
});
