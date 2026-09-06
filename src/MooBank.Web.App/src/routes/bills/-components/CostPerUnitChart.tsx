import React from "react";
import type { ChartData } from "chart.js";
import { Line } from "react-chartjs-2";
import { Section } from "@andrewmclachlan/moo-ds";

import type { BillFilter } from "../-hooks/types";
import { useCostPerUnitReport } from "../-hooks/useCostPerUnitReport";
import { chartColours, useChartColours } from "utils/chartColours";
import { costPerUnitChartData } from "../-utils/billCharts";


export interface CostPerUnitChartProps {
    utilityType?: string;
    filter: BillFilter;
}

export const CostPerUnitChart: React.FC<CostPerUnitChartProps> = ({ utilityType, filter }) => {
    const colours = useChartColours();

    const { data: report } = useCostPerUnitReport(
        filter.startDate ?? "",
        filter.endDate ?? "",
        filter.accountId,
        utilityType
    );

    const { dates, series } = costPerUnitChartData(report?.dataPoints ?? []);

    const dataset: ChartData<"line", (number | null)[], string> = {
        labels: dates,
        datasets: series.map((s, index) => ({
            label: s.label,
            data: s.data,
            backgroundColor: chartColours[index % chartColours.length],
            borderColor: chartColours[index % chartColours.length],
            tension: 0.1,
        }))
    };

    return (
        <Section className="bill-chart" header="Cost Per Unit">
            <div className="chart-container">
                <Line
                    data={dataset}
                    options={{
                        maintainAspectRatio: false,
                        scales: {
                            y: {
                                suggestedMin: 0,
                                title: {
                                    display: true,
                                    text: "Price Per Unit ($)"
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
                                display: series.length > 1
                            }
                        }
                    }}
                />
            </div>
        </Section>
    );
};

CostPerUnitChart.displayName = "CostPerUnitChart";
