import React, { useEffect, useMemo, useRef } from "react";

import { useBreakdownReport } from "../../../-hooks/useBreakdownReport";

import type { ChartData, ChartOptions } from "chart.js";
import { Doughnut } from "react-chartjs-2";
import { Skeleton } from "@andrewmclachlan/moo-ds";

import type { Period } from "models/dateFns";
import { chartColours } from "utils/chartColours";
import type { transactionTypeFilter } from "models/transactions";
import type { TagValue } from "api/types.gen";
import { useElementWidth } from "hooks";
import { doughnutLegendPosition } from "../-utils/legendPosition";


export const Breakdown: React.FC<BreakdownProps> = ({ accountId, tagId, period, reportType, selectedTagChanged }) => {

    const report = useBreakdownReport(accountId!, period?.startDate, period?.endDate, reportType, tagId);

    const chartRef = useRef(null);
    const [containerRef, containerWidth] = useElementWidth();

    useEffect(() => {
        if (chartRef.current) {
            (chartRef.current as any)._hiddenIndices = {};
        }
        chartRef.current?.getDatasetMeta(0).data.forEach((_bar: any, i: any) => {
            chartRef.current?.setDatasetVisibility(i, true);
        });

        // Optional: the chart is unmounted while the report loads, so this runs
        // with a null ref on the first pass.
        //
        // "none": this restores legend visibility and changes no value, so there is nothing to
        // animate between. The default mode would replay the whole rotation.
        chartRef.current?.update("none");
    }, [tagId]);

    const dataset: ChartData<"doughnut", number[], string> = useMemo(() => {
        return {
            labels: report.data?.tags.map(t => t.tagName) ?? [],
            datasets: [{
                label: "",
                data: report.data?.tags.map(t => t.netAmount!) ?? [],
                backgroundColor: chartColours,
                borderColor: "#FFFCFC55",
            }],
        };
    }, [report.data, accountId, period?.startDate, period?.endDate, reportType, tagId]);

    const legendPosition = doughnutLegendPosition(containerWidth);

    // What the click handler needs, read through a ref so that options need not close over it.
    //
    // react-chartjs-2 keys its update effect on the identity of the options object, and Chart.js
    // animates every update. Callers pass a fresh selectedTagChanged on each render, so options
    // that closed over it would be rebuilt -- and the chart redrawn -- whenever the parent renders.
    // This leaves options depending on the legend position alone.
    const latestRef = useRef({ tags: report.data?.tags, selectedTagChanged });
    useEffect(() => {
        latestRef.current = { tags: report.data?.tags, selectedTagChanged };
    });

    const options = useMemo<ChartOptions<"doughnut">>(() => ({
        maintainAspectRatio: false,
        plugins: {
            legend: {
                position: legendPosition,
            },
            tooltip: {
                mode: "point",
                intersect: false,
            } as any,
        },
        hover: {
            mode: "point",
            intersect: true,
        },
        onClick: (_event, elements) => {
            if (elements.length !== 1) return;
            const tag = latestRef.current.tags?.[elements[0].index];
            if (tag) latestRef.current.selectedTagChanged?.(tag);
        },
    }), [legendPosition]);

    // The chart's shape is known before its data is, so hold the space with a
    // skeleton rather than a spinner. Returning early also keeps an empty
    // <Doughnut> from being painted underneath the placeholder while it loads.
    if (report.isLoading) return <Skeleton.Chart variant="doughnut" count={3} />;

    // Withheld until the container has been measured, so the chart is created once with its legend
    // already in place. Creating it at the default position and moving the legend afterwards is a
    // real options change, and Chart.js animates it. The div carries the ref in both branches and
    // holds its place in the tree, so switching between them does not restart the measurement.
    return (
        <div ref={containerRef} className="doughnut-container">
            {containerWidth === null
                ? <Skeleton.Chart variant="doughnut" count={3} />
                : <Doughnut id="bytag" ref={chartRef} data={dataset} options={options} />}
        </div>
    );
}

export interface BreakdownProps {
    accountId: string;
    tagId?: number;
    period: Period;
    reportType: transactionTypeFilter;
    selectedTagChanged?: (tag: TagValue) => void;
}

Breakdown.displayName = "Breakdown";
