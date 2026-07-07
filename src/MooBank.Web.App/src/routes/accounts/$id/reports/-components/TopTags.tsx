import { useState } from "react";

import { useAllTagAverageReport } from "../../../-hooks/useAllTagAverageReport";

import type { ChartData } from "chart.js";
import { Bar } from "react-chartjs-2";

import type { Period } from "models/dateFns";
import { useNavigate } from "@tanstack/react-router";
import { chartColours, desaturatedChartColours } from "utils/chartColours";
import type { transactionTypeFilter } from "store/state";

export const TopTags: React.FC<TopTagsProps> = ({ accountId, period, reportType, top = 20, periodId }) => {

    const navigate = useNavigate();
    const [showGross] = useState<boolean>(false); //TODO: may make this an option

    const report = useAllTagAverageReport(accountId, period?.startDate, period?.endDate, reportType, top);

    const dataset: ChartData<"bar", number[], string> = {
        labels: report.data?.tags.map(t => t.tagName) ?? [],
        datasets: [{
            label: "",
            data: report.data?.tags.map(t => t.netAmount!) ?? [],
            backgroundColor: chartColours,
        }],
    };

    if (showGross) {
        dataset.datasets.push({
            label: "",
            data: report.data?.tags.map(t => t.grossAmount!) ?? [],
            backgroundColor: desaturatedChartColours,
        });
    }

    return (
        <Bar id="alltagaverage" data={dataset} options={{
            plugins: {
                legend: {
                    display: false,
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
            scales: {
                x: {
                    stacked: true
                }
            },
            maintainAspectRatio: false,
            onClick: (_event, elements) => {
                if (elements.length !== 1) return;
                const tag = report.data!.tags[elements[0].index];
                const periodQuery = periodId ? `&period=${periodId}` : "";
                const url = !tag.tagId ? `/accounts/${accountId}?untagged=true${periodQuery}` : `/accounts/${accountId}?tag=${tag.tagId}&type=${reportType}${periodQuery}`;
                navigate({ to: url });
            },
        }}
        />
    );
}

export interface TopTagsProps {
    accountId: string;
    period: Period;
    reportType: transactionTypeFilter;
    top?: number;
    /** When set, the period option id (e.g. "1" = Last Month) is appended to the transactions drilldown URL to scope it. */
    periodId?: string;
}
