import { describe, it, expect } from "vitest";
import { colourRowProps } from "./colourRow";

describe("colourRowProps", () => {

    it("carries the colour through as the custom property the CSS reads", () => {
        const props = colourRowProps("#ff8800");

        expect(props.className).toContain("colour-row");
        expect(props.className).toContain("has-colour");
        expect(props.style).toEqual({ "--row-colour": "#ff8800" });
    });

    it("marks a row with no colour so the edge stays reserved but empty", () => {
        const props = colourRowProps(null);

        // colour-row still applies: it reserves the border width so rows do not shift
        // sideways when a colour is set or cleared. has-colour is what paints it.
        expect(props.className).toContain("colour-row");
        expect(props.className).not.toContain("has-colour");
        expect(props.style).toBeUndefined();
    });

    it.each([undefined, null, "", 0, {}])("treats %o as no colour", (colour) => {
        expect(colourRowProps(colour).className).not.toContain("has-colour");
    });

    it("keeps a caller's own class", () => {
        // Group rows are clickable; losing that would silently drop the row's hover
        // affordance and its navigation styling.
        const props = colourRowProps("#ff8800", "clickable");

        expect(props.className.split(" ")).toEqual(["clickable", "colour-row", "has-colour"]);
    });

    it("produces no stray whitespace when there is no caller class", () => {
        expect(colourRowProps(null).className).toBe("colour-row");
    });
});
