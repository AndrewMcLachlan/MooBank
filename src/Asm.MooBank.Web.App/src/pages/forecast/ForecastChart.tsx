import { Section } from "@andrewmclachlan/moo-ds";
import { format, parseISO } from "date-fns";
import {
    Chart as ChartJS,
    CategoryScale,
    LinearScale,
    PointElement,
    LineElement,
    Title,
    Tooltip,
    Legend,
    ChartOptions
} from "chart.js";
import { Line } from "react-chartjs-2";
import type { ForecastMonth } from "api/types.gen";

ChartJS.register(
    CategoryScale,
    LinearScale,
    PointElement,
    LineElement,
    Title,
    Tooltip,
    Legend
);

interface ForecastChartProps {
    months: ForecastMonth[];
}

export const ForecastChart: React.FC<ForecastChartProps> = ({ months }) => {

    const labels = months.map(m => format(parseISO(m.monthStart), "MMM yy"));

    const data = {
        labels,
        datasets: [
            {
                label: "Projected Balance",
                data: months.map(m => m.openingBalance),
                borderColor: "rgb(53, 162, 235)",
                backgroundColor: "rgba(53, 162, 235, 0.5)",
                tension: 0.1
            },
            {
                label: "Actual Balance",
                data: months.map(m => m.actualBalance ?? null),
                borderColor: "rgb(34, 197, 94)",
                backgroundColor: "rgba(34, 197, 94, 0.5)",
                tension: 0.1,
                spanGaps: false
            }
        ]
    };

    const options: ChartOptions<"line"> = {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
            legend: {
                position: "top" as const,
            },
            title: {
                display: false,
            },
            tooltip: {
                callbacks: {
                    label: (context) => {
                        const value = context.parsed.y;
                        return `${context.dataset.label}: $${value.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`;
                    }
                }
            }
        },
        scales: {
            y: {
                ticks: {
                    callback: (value) => `$${Number(value).toLocaleString()}`
                }
            }
        }
    };

    return (
        <Section header="Balance Projection" className="forecast-chart">
            <div style={{ height: "400px" }}>
                <Line options={options} data={data} />
            </div>
        </Section>
    );
};
