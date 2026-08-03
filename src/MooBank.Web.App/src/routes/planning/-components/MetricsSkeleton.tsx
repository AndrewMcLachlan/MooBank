import { Skeleton } from "@andrewmclachlan/moo-ds";
import { Kpi, KpiSub, KpiValue } from "components/Kpi";

/**
 * Stand-in for a metrics strip while its projection loads.
 *
 * Built from the same `Kpi` the real cards use, so the placeholder inherits every rule that sizes
 * them — padding, type scale, margins — rather than restating any of it. Restating them is what
 * left an earlier version a few pixels short in every card, which is plainly visible once it is
 * multiplied across a row and everything below shifts up.
 */
export const MetricsSkeleton: React.FC<MetricsSkeletonProps> = ({ className, count }) => (
    <div className={className} role="status" aria-label="Loading">
        {Array.from({ length: count }, (_, i) =>
            <Kpi label={<Skeleton.Text />} key={i}>
                <KpiValue><Skeleton.Text /></KpiValue>
                <KpiSub><Skeleton.Text /></KpiSub>
            </Kpi>
        )}
    </div>
);

export interface MetricsSkeletonProps {
    /** The grid class that owns the layout — `forecast-metrics` or `retirement-metrics`. */
    className: string;
    count: number;
}
