import { addMonths, format, parseISO, startOfMonth } from "date-fns";
import { Widget } from "@andrewmclachlan/moo-ds";
import type { ChartOptions } from "chart.js";
import { Line } from "react-chartjs-2";

import { useChartColours } from "utils/chartColours";
import { formatCurrency } from "utils/currency";
import { WidgetError } from "components/WidgetError";
import { useUser } from "hooks/useUser";
import { useForecastPlans } from "../forecast/-hooks/useForecastPlans";
import { useForecastPlan } from "../forecast/-hooks/useForecastPlan";
import { useForecastResult } from "../forecast/-hooks/useForecastResult";

const MONTHS_BEHIND = 6;
const MONTHS_AHEAD = 6;

export const ForecastWidget: React.FC = () => {

    const { data: plans, isLoading: plansLoading, isError: plansError } = useForecastPlans();
    const plan = plans?.[0];
    const planId = plan?.id ?? "";

    const { data: fullPlan, isError: planError } = useForecastPlan(planId);
    const { data: result, isFetching, isError: runError } = useForecastResult(planId);
    const { data: user } = useUser();
    const colours = useChartColours();

    // The forecast is denominated in the plan's currency, falling back to the user's preferred currency.
    const currencyCode = fullPlan?.currencyCode ?? user?.currency ?? "AUD";

    // No forecast plan exists at all - don't render the widget
    if (!plansLoading && !plansError && plans && plans.length === 0) {
        return null;
    }

    const header = plan ? `Forecast - ${plan.name}` : "Forecast";
    const hasError = plansError || planError || runError;

    if (hasError) {
        return (
            <Widget header={header} size="double" headerSize={2} className="report forecast-widget" to="/forecast">
                <WidgetError />
            </Widget>
        );
    }

    const today = startOfMonth(new Date());
    const windowStart = addMonths(today, -MONTHS_BEHIND);
    const windowEnd = addMonths(today, MONTHS_AHEAD);

    const months = (result?.months ?? []).filter(m => {
        const monthDate = parseISO(m.monthStart);
        return monthDate >= windowStart && monthDate <= windowEnd;
    });

    const labels = months.map(m => format(parseISO(m.monthStart), "MMM yy"));

    const data = {
        labels,
        datasets: [
            {
                label: "Projected Balance",
                data: months.map(m => m.openingBalance),
                borderColor: "rgb(53, 162, 235)",
                backgroundColor: "rgba(53, 162, 235, 0.5)",
                tension: 0.1,
            },
            {
                label: "Actual Balance",
                data: months.map(m => m.actualBalance ?? null),
                borderColor: colours.income,
                backgroundColor: colours.incomeTrend,
                tension: 0.1,
                spanGaps: false,
            },
        ],
    };

    const options: ChartOptions<"line"> = {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
            legend: { position: "top" },
            tooltip: {
                callbacks: {
                    label: (context) => {
                        const value = context.parsed.y;
                        return `${context.dataset.label}: ${formatCurrency(value, currencyCode)}`;
                    },
                },
            },
        },
        scales: {
            y: {
                grid: { color: colours.grid },
                ticks: {
                    callback: (value) => formatCurrency(Number(value), currencyCode, 0),
                },
            },
            x: {
                grid: { color: colours.grid },
            },
        },
    };

    return (
        <Widget header={header} size="double" headerSize={2} className="report forecast-widget" loading={plansLoading || isFetching} to="/forecast">
            <div className="forecast-widget-chart">
                <Line data={data} options={options} />
            </div>
        </Widget>
    );
};
