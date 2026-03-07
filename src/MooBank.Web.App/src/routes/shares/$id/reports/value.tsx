import { Input, Section } from "@andrewmclachlan/moo-ds";
import { createFileRoute } from "@tanstack/react-router";
import { ChartData } from "chart.js";
import React, { useState } from "react";
import { Line } from "react-chartjs-2";
import { useParams } from "@tanstack/react-router";

import { PeriodSelector } from "components/PeriodSelector";
import { formatDate } from "date-fns/format";
import type { Period } from "models/dateFns";
import { useStockValueReport } from "routes/shares/-hooks/useStockValueReport";
import { useChartColours } from "utils/chartColours";
import { ReportsPage } from "./-components/ReportsPage";


export const Route = createFileRoute("/shares/$id/reports/value")({
    component: StockValueReport,
});

function StockValueReport() {

    const colours = useChartColours();

    const { id: accountId } = useParams({ strict: false });

    const [period, setPeriod] = useState<Period>({ startDate: null, endDate: null });
    const [yAxisFromZero, setYAxisFromZero] = useState<boolean>(false);
    const report = useStockValueReport(accountId!, period?.startDate, period?.endDate);

    const dataset: ChartData<"line", number[], string> = {
        labels: report.data?.points.map(i => formatDate(i.date, "dd/MM/yyyy")) ?? [],
        datasets: [{
            label: `${report.data?.symbol.trimEnd()} Value`,
            data: report.data?.points.map(i => i.value) ?? [],
            backgroundColor: colours.income,
            borderColor: colours.income,
            yAxisID: "y",

        },
        {
            label: `Invested Amount`,
            data: report.data?.investment.map(i => i.value) ?? [],
            backgroundColor: colours.expenses,
            borderColor: colours.expenses,
            //yAxisID: "y2",
        }]
    };

    const max = dataset.datasets[0].data.reduce((a, b) => Math.max(a, b), 0);

    return (
        <ReportsPage title="Value Trend">
            <Section>
                <div>
                    <PeriodSelector onChange={setPeriod} instant />
                    <Input.Check type="checkbox" label="Y Axis from 0" onChange={e => setYAxisFromZero(e.currentTarget.checked)} />
                </div>
            </Section>
            <Section className="report" header="Value Trend" headerSize={3}>
                <Line id="inout" data={dataset} options={{
                    maintainAspectRatio: true,
                    scales: {
                        y: {
                            suggestedMin: yAxisFromZero ? 0 : dataset.datasets[0].data.reduce((a, b) => Math.min(a, b), max),
                            ticks: {
                                stepSize: 1000,
                            },
                            grid: {
                                color: colours.grid,
                            },
                        },
                        /*y2: {
                            suggestedMin: dataset.datasets[1].data.reduce((a, b) => Math.min(a, b), 1000000000),
                            ticks: {
                                stepSize: 1000,
                            },
                            grid: {
                                color: colours.grid,
                            },
                        },*/
                        x: {
                            grid: {
                                color: colours.grid,
                            },
                        }
                    }
                }} />
            </Section>
        </ReportsPage>
    );
}
