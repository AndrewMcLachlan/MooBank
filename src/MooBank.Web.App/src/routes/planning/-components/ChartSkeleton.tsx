import { Section, Skeleton } from "@andrewmclachlan/moo-ds";

/**
 * Stand-in for a chart while its data loads.
 *
 * The charts bail out to `null` on empty data, which is right once loaded but leaves a hole during
 * the wait — the sections below start high and drop when the data lands. The canvas class is passed
 * in rather than restated, because that class is where the chart's fixed height already lives.
 */
export const ChartSkeleton: React.FC<ChartSkeletonProps> = ({ header, canvasClassName }) => (
    <Section header={header} role="status" aria-label="Loading">
        <div className={canvasClassName}>
            <Skeleton.Rect className="skeleton-fill" />
        </div>
    </Section>
);

export interface ChartSkeletonProps {
    header: string;
    /** The class carrying the chart's height — e.g. `forecast-mini-chart`, `retirement-chart-canvas`. */
    canvasClassName: string;
}
