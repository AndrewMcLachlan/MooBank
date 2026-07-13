import { describe, it, expect } from "vitest";
import { overUnderBudgetColours } from "utils/chartColours";

const colours = { income: "green", expenses: "red" };

describe("overUnderBudgetColours", () => {
    it("returns the expenses colour when the actual exceeds the budgeted amount", () => {
        expect(overUnderBudgetColours([100], [80], colours)).toEqual(["red"]);
    });

    it("returns the income colour when the actual is under the budgeted amount", () => {
        expect(overUnderBudgetColours([40], [80], colours)).toEqual(["green"]);
    });

    it("returns the income colour when the actual exactly equals the budgeted amount", () => {
        expect(overUnderBudgetColours([50], [50], colours)).toEqual(["green"]);
    });

    it("compares the absolute value of the actual against the budgeted amount", () => {
        expect(overUnderBudgetColours([-100], [80], colours)).toEqual(["red"]);
        expect(overUnderBudgetColours([-40], [80], colours)).toEqual(["green"]);
    });

    it("treats a missing budgeted entry as 0", () => {
        expect(overUnderBudgetColours([0, -5], [80], colours)).toEqual(["green", "red"]);
    });

    it("maps each actual/budgeted pair independently across the arrays", () => {
        expect(overUnderBudgetColours([100, 50, 50], [80, 50, 60], colours)).toEqual(["red", "green", "green"]);
    });
});
