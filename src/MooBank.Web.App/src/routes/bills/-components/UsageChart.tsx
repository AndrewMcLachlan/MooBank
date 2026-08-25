import React, { useMemo } from "react";
import type { ChartData } from "chart.js";
import type { AnnotationOptions } from "chartjs-plugin-annotation";
import { Line } from "react-chartjs-2";
import { Section } from "@andrewmclachlan/moo-ds";

import type { BillFilter } from "../-hooks/types";
import { useUsageReport } from "../-hooks/useUsageReport";
import { chartColours, useChartColours } from "utils/chartColours";
import { rollingAverage, usageChartData } from "../-utils/billCharts";


export interface UsageChartProps {
    utilityType?: string;
    filter: BillFilter;
    rollingWindowSize?: number;
}

export const UsageChart: React.FC<UsageChartProps> = ({ utilityType, filter, rollingWindowSize = 3 }) => {
    const colours = useChartColours();

    const { data: usageReport } = useUsageReport(
        filter.startDate ?? "",
        filter.endDate ?? "",
        filter.accountId,
        utilityType
    );

    const { dates: allDates, consumption: rawData, export: exportData, hasExport } = useMemo(
        () => usageChartData(usageReport?.dataPoints ?? []),
        [usageReport?.dataPoints]);

    const consumptionPoints = usageReport?.dataPoints.filter(d => d.usageType === "Consumption") ?? [];

    const rollingAverageData = useMemo(() => {
        return rollingAverage(rawData, rollingWindowSize);
    }, [rawData, rollingWindowSize]);

    const accountChangeAnnotations = useMemo(() => {
        const dataPoints = consumptionPoints;
        if (dataPoints.length < 2) return {};

        const annotations: Record<string, AnnotationOptions<"line">> = {};
        let previousAccount = dataPoints[0]?.accountName;

        for (let i = 1; i < dataPoints.length; i++) {
            const currentAccount = dataPoints[i].accountName;
            if (currentAccount !== previousAccount) {
                const dateIndex = allDates.indexOf(dataPoints[i].date);
                annotations[`accountChange${i}`] = {
                    type: "line",
                    xMin: dateIndex - 0.5,
                    xMax: dateIndex - 0.5,
                    borderColor: colours.income,
                    borderWidth: 2,
                    borderDash: [6, 6],
                    label: {
                        display: true,
                        content: currentAccount,
                        position: "start",
                    },
                };
                previousAccount = currentAccount;
            }
        }

        return annotations;
    }, [usageReport?.dataPoints, allDates, colours.income]);

    const dataset: ChartData<"line", (number | null)[], string> = {
        labels: allDates,
        datasets: [
            {
                label: hasExport ? "Consumption/Day" : "Usage/Day",
                data: rawData,
                backgroundColor: chartColours[2],
                borderColor: chartColours[2],
                borderWidth: 1,
                pointRadius: 3,
                tension: 0,
                yAxisID: "y",
            },
            ...(hasExport ? [{
                label: "Export/Day",
                data: exportData,
                backgroundColor: chartColours[4],
                borderColor: chartColours[4],
                borderWidth: 1,
                pointRadius: 3,
                tension: 0,
                yAxisID: "y",
            }] : []),
            {
                label: `${rollingWindowSize}-Period Average`,
                data: rollingAverageData,
                backgroundColor: chartColours[6],
                borderColor: chartColours[6],
                borderWidth: 2,
                pointRadius: 0,
                tension: 0.3,
                yAxisID: "y",
            },
        ]
    };

    return (
        <Section className="bill-chart" header="Usage Trends">
            <div className="chart-container">
                <Line
                    data={dataset}
                    options={{
                        maintainAspectRatio: false,
                        interaction: {
                            mode: "index",
                            intersect: false,
                        },
                        scales: {
                            y: {
                                type: "linear",
                                display: true,
                                position: "left",
                                suggestedMin: 0,
                                title: {
                                    display: true,
                                    text: "Usage Per Day"
                                },
                                grid: {
                                    color: colours.grid,
                                },
                            },
                            x: {
                                grid: {
                                    color: colours.grid,
                                },
                            }
                        },
                        plugins: {
                            legend: {
                                display: true,
                                position: "bottom",
                            },
                            annotation: {
                                annotations: accountChangeAnnotations,
                            },
                        }
                    }}
                />
            </div>
        </Section>
    );
};

UsageChart.displayName = "UsageChart";
