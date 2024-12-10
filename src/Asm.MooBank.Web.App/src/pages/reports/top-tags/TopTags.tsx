import { useRef, useState } from "react";

import { useAllTagAverageReport } from "services";

import { ChartData } from "chart.js";
import { Bar, getElementAtEvent } from "react-chartjs-2";

import { Period } from "helpers/dateFns";
import { useNavigate } from "react-router";
import { chartColours, desaturatedChartColours } from "../../../helpers/chartColours";
import { ReportType } from "models/reports";

export const TopTags: React.FC<TopTagsProps> = ({ accountId, period, reportType, top = 20 }) => {

    const navigate = useNavigate();
    const [showGross] = useState<boolean>(false); //TODO: may make this an option

    const report = useAllTagAverageReport(accountId, period?.startDate, period?.endDate, reportType, top);

    const chartRef = useRef(null);

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
        <Bar id="alltagaverage" ref={chartRef} data={dataset} options={{
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
        }}
            onClick={(e) => {
                const elements = getElementAtEvent(chartRef.current!, e);
                if (elements.length !== 1) return;
                if (!report.data!.tags[elements[0].index].hasChildren) return;
                navigate(`/accounts/${accountId}/reports/breakdown/${report.data!.tags[elements[0].index].tagId}`);
            }}
        />
    );
}

export interface TopTagsProps {
    accountId: string;
    period: Period;
    reportType: ReportType;
    top?: number;
}
