import React, { useMemo } from "react";
import { ChartData, Chart as ChartJS, registerables } from "chart.js";
import annotationPlugin, { AnnotationOptions } from "chartjs-plugin-annotation";
import { Line } from "react-chartjs-2";
import { Section } from "@andrewmclachlan/moo-ds";

import type { BillFilter, CostDataPoint } from "../-hooks/types";
import { useCostPerUnitReport } from "../-hooks/useCostPerUnitReport";
import { useServiceChargeReport } from "../-hooks/useServiceChargeReport";
import { chartColours, useChartColours } from "utils/chartColours";

ChartJS.register(...registerables, annotationPlugin);

export interface BillsChartProps {
    utilityType?: string;
    filter: BillFilter;
}

export const BillsChart: React.FC<BillsChartProps> = ({ utilityType, filter }) => {
    const colours = useChartColours();

    const { data: costReport } = useCostPerUnitReport(
        filter.startDate ?? "",
        filter.endDate ?? "",
        filter.accountId,
        utilityType
    );

    const { data: chargeReport } = useServiceChargeReport(
        filter.startDate ?? "",
        filter.endDate ?? "",
        filter.accountId,
        utilityType
    );

    const allDates = [...new Set([
        ...(costReport?.dataPoints.map(d => d.date) ?? []),
        ...(chargeReport?.dataPoints.map(d => d.date) ?? [])
    ])].sort();

    const accountChangeAnnotations = useMemo(() => {
        const dataPoints = costReport?.dataPoints ?? [];
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
    }, [costReport?.dataPoints, allDates, colours.grid]);

    const dataset: ChartData<"line", (number | null)[], string> = {
        labels: allDates,
        datasets: [
            {
                label: "Cost/Unit",
                data: allDates.map(date => {
                    const point = costReport?.dataPoints.find(d => d.date === date);
                    return point?.averagePricePerUnit ?? null;
                }),
                backgroundColor: chartColours[1],
                borderColor: chartColours[1],
                tension: 0.1,
                yAxisID: "y",
            },
            {
                label: "Service/Day",
                data: allDates.map(date => {
                    const point = chargeReport?.dataPoints.find(d => d.date === date);
                    return point?.averageChargePerDay ?? null;
                }),
                backgroundColor: chartColours[6],
                borderColor: chartColours[6],
                borderDash: [5, 5],
                tension: 0.1,
                yAxisID: "y1",
            },
        ]
    };

    return (
        <Section className="bill-chart" header="Cost Trends">
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
                                    text: "Cost Per Unit ($)"
                                },
                                grid: {
                                    color: colours.grid,
                                },
                            },
                            y1: {
                                type: "linear",
                                display: true,
                                position: "right",
                                suggestedMin: 0,
                                title: {
                                    display: true,
                                    text: "Service Charge/Day ($)"
                                },
                                grid: {
                                    drawOnChartArea: false,
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

BillsChart.displayName = "BillsChart";
