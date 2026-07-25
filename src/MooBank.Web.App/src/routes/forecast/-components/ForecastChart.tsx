import { Line } from "react-chartjs-2";
import type { ForecastMonth } from "api/types.gen";
import { useChartColours } from "utils/chartColours";
import { forecastChartData, forecastChartOptions } from "../-utils/forecastChart";

interface ForecastChartProps {
    months: ForecastMonth[];
    currencyCode: string;
}

export const ForecastChart: React.FC<ForecastChartProps> = ({ months, currencyCode }) => {

    const colours = useChartColours();

    return (
        <div className="forecast-chart-canvas">
            <Line data={forecastChartData(months, colours)} options={forecastChartOptions(currencyCode, colours)} />
        </div>
    );
};
