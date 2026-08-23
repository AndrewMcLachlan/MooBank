import type { LayoutPosition } from "chart.js";

/**
 * Below this the ring is worth more than the legend's place beside it. Chart.js gives a right-hand
 * legend up to about a third of the width, so a narrower box leaves the doughnut under ~200px.
 */
export const legendSideMinWidth = 500;

/**
 * Where a doughnut's legend goes for a given container width. Null means the container has not been
 * measured yet, which keeps the legend where it renders on a full-width report until it has been.
 */
export const doughnutLegendPosition = (containerWidth: number | null): LayoutPosition =>
    containerWidth !== null && containerWidth < legendSideMinWidth ? "bottom" : "right";
