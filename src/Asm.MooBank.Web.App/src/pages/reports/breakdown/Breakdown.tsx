import React, { useEffect, useMemo, useRef, useState } from "react";

import { useBreakdownReport, useTag } from "services";

import { ChartData, Chart as ChartJS, LegendItem, registerables } from "chart.js";
import { Doughnut, getElementAtEvent } from "react-chartjs-2";

import { Period } from "helpers/dateFns";
import { useNavigate, useParams } from "react-router";
import { chartColours } from "../../../helpers/chartColours";
import { transactionTypeFilter } from "store/state";

ChartJS.register(...registerables);

export const Breakdown: React.FC<BreakdownProps> = ({ accountId, tagId, period, reportType, selectedTagChanged }) => {

    const navigate = useNavigate();

    const report = useBreakdownReport(accountId!, period?.startDate, period?.endDate, reportType, tagId);

    const chartRef = useRef(null);

    useEffect(() => {
        if (chartRef.current) {
            (chartRef.current as any)._hiddenIndices = {};
        }
        chartRef.current?.getDatasetMeta(0).data.forEach((_bar: any, i: any) => {
            chartRef.current?.setDatasetVisibility(i, true);
        });

        chartRef.current.update();
    }, [tagId]);

    const dataset: ChartData<"doughnut", number[], string> = useMemo(() => {
        return {
            labels: report.data?.tags.map(t => t.tagName) ?? [],
            datasets: [{
                label: "",
                data: report.data?.tags.map(t => t.netAmount) ?? [],
                backgroundColor: chartColours,
                borderColor: "#FFFCFC55",
            }],
        };
    }, [report.data, accountId, period?.startDate, period?.endDate, reportType, tagId]);

    return (
        <Doughnut id="bytag" className="bob" ref={chartRef} data={dataset} options={{
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    position: "right"
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
        }}
            onClick={(e) => {
                const elements = getElementAtEvent(chartRef.current!, e);
                if (elements.length !== 1) return;
                const tag = report.data!.tags[elements[0].index];
                if (!tag.hasChildren || tag.tagId === tagId) {

                    const url = !tag.tagId ? `/accounts/${accountId}?untagged=true` : `/accounts/${accountId}?tag=${tag.tagId}&type=${reportType}`;

                    navigate(url);
                    return;
                }
                selectedTagChanged?.(tag.tagId);
                const newUrl = `/accounts/${accountId}/reports/breakdown/${tag.tagId}`;
                window.history.replaceState({ path: newUrl }, "", newUrl);
            }} />
    );
}

export interface BreakdownProps {
    accountId: string;
    tagId?: number;
    period: Period;
    reportType: transactionTypeFilter;
    selectedTagChanged?: (tagId: number) => void;
}

Breakdown.displayName = "Breakdown";