import { describe, it, expect } from "vitest";
import { doughnutLegendPosition, legendSideMinWidth } from "./legendPosition";

describe("doughnutLegendPosition", () => {
    it("keeps the legend beside the ring when there is room", () => {
        expect(doughnutLegendPosition(900)).toBe("right");
        expect(doughnutLegendPosition(legendSideMinWidth)).toBe("right");
    });

    it("moves it below when the container is narrow, so the ring gets the width", () => {
        expect(doughnutLegendPosition(legendSideMinWidth - 1)).toBe("bottom");
        expect(doughnutLegendPosition(320)).toBe("bottom");
    });

    it("stays beside the ring before the container has been measured", () => {
        // The first render has no measurement. Defaulting to bottom would make every chart flick
        // from stacked to side-by-side on load, including the ones that were always wide enough.
        expect(doughnutLegendPosition(null)).toBe("right");
    });
});
