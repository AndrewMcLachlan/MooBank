import type { CSSProperties } from "react";

/**
 * Row props that show an entity's colour as an edge on its management row.
 *
 * Returns the class and custom property the `.colour-row` styles read (see
 * `css/components/colourrow.css`). The colour is omitted rather than defaulted when
 * unset, so an entity with no colour shows a reserved but empty edge instead of
 * borrowing someone else's.
 *
 * `colour` is typed loosely because the generated `HexColour` is `unknown`.
 */
export const colourRowProps = (colour: unknown, className?: string) => {
    const hasColour = typeof colour === "string" && colour.length > 0;

    return {
        className: [className, "colour-row", hasColour ? "has-colour" : null].filter(Boolean).join(" "),
        style: hasColour ? ({ "--row-colour": colour } as CSSProperties) : undefined,
    };
};
