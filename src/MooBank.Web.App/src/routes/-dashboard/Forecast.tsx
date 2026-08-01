import { addMonths, parseISO, startOfMonth } from "date-fns";
import { Widget } from "@andrewmclachlan/moo-ds";
import { Line } from "react-chartjs-2";

import { useChartColours } from "utils/chartColours";
import { WidgetError } from "components/WidgetError";
import { forecastChartData, forecastChartOptions } from "../planning/-utils/forecastChart";
import { useUser } from "hooks/useUser";
import { useForecastPlans } from "../planning/-hooks/useForecastPlans";
import { useForecastPlan } from "../planning/-hooks/useForecastPlan";
import { useForecastResult } from "../planning/-hooks/useForecastResult";

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
            <Widget header={header} size="double" headerSize={2} className="report forecast-widget" to="/planning/forecast">
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

    const data = forecastChartData(months, colours);
    const options = forecastChartOptions(currencyCode, colours);

    return (
        <Widget header={header} size="double" headerSize={2} className="report forecast-widget" loading={plansLoading || isFetching} to="/planning/forecast">
            <div className="forecast-widget-chart">
                <Line data={data} options={options} />
            </div>
        </Widget>
    );
};
