import { describe, it, expect } from "vitest";
import { getUnit } from "utils/units";

describe("getUnit", () => {
    it("returns kWh for Electricity", () => {
        expect(getUnit("Electricity")).toBe("kWh");
    });

    it("returns kL for Water", () => {
        expect(getUnit("Water")).toBe("kL");
    });

    it("returns an empty string for other utility types", () => {
        expect(getUnit("Gas")).toBe("");
        expect(getUnit("Phone")).toBe("");
        expect(getUnit("Internet")).toBe("");
        expect(getUnit("Other")).toBe("");
    });
});
