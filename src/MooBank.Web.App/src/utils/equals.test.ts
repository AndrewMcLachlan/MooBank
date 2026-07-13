import { describe, it, expect } from "vitest";
import { equals, notEquals } from "utils/equals";

describe("equals", () => {
    it("is true for identical numbers", () => {
        expect(equals(1, 1)).toBe(true);
        expect(equals(0, 0)).toBe(true);
        expect(equals(-5.5, -5.5)).toBe(true);
    });

    it("is true for differences within epsilon (1e-4)", () => {
        expect(equals(1.00005, 1.0)).toBe(true);
    });

    it("is false for differences outside epsilon (1e-4)", () => {
        expect(equals(1.0002, 1.0)).toBe(false);
    });

    it("is symmetric", () => {
        expect(equals(1.0, 1.00005)).toBe(true);
        expect(equals(1.0, 1.0002)).toBe(false);
    });
});

describe("notEquals", () => {
    it("is the inverse of equals", () => {
        expect(notEquals(1.00005, 1.0)).toBe(false);
        expect(notEquals(1.0002, 1.0)).toBe(true);
    });
});
