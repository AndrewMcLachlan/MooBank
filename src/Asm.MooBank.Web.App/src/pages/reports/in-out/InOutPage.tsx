import { useState } from "react";

import { ReportsPage } from "../ReportsPage";

import { Section, useIdParams } from "@andrewmclachlan/mooapp";
import { Chart as ChartJS, registerables } from "chart.js";
import chartTrendline from "chartjs-plugin-trendline";

import { PeriodSelector } from "components/PeriodSelector";
import { Period } from "helpers/dateFns";
import { InOut } from "./InOut";
import { InOutTrend } from "./InOutTrend";

ChartJS.register(...registerables);
ChartJS.register(chartTrendline);

export const InOutPage = () => {

    const accountId = useIdParams();

    const [period, setPeriod] = useState<Period>({ startDate: null, endDate: null });

   return (
        <ReportsPage title="Income vs Expenses">
            <Section>
            <PeriodSelector onChange={setPeriod} />
            </Section>
            <Section title="Total Income vs Expenses" titleSize={3} className="report inout">
                <InOut accountId={accountId} period={period} />
            </Section>
            <Section title="Income vs Expenses per Month" titleSize={3} className="report inout-trend">
                <InOutTrend accountId={accountId} period={period} />
            </Section>
        </ReportsPage>
    );

}
