import { Line } from "react-chartjs-2";
import { Section } from "@andrewmclachlan/moo-ds";
import type { RetirementProjectionYear } from "api/types.gen";
import { useChartColours } from "utils/chartColours";
import { retirementChartData, retirementChartOptions } from "../-utils/retirementChart";

interface RetirementChartProps {
    years: RetirementProjectionYear[];
    currencyCode: string;
}

export const RetirementChart: React.FC<RetirementChartProps> = ({ years, currencyCode }) => {

    const colours = useChartColours();

    if (years.length === 0) return null;

    return (
        <Section header="Projected Balance">
            <div className="retirement-chart-canvas">
                <Line data={retirementChartData(years, colours)} options={retirementChartOptions(currencyCode, colours)} />
            </div>
        </Section>
    );
};
