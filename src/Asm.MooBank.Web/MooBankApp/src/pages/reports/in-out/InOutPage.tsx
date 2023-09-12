import React, { useState } from "react";

import { ReportsPage } from "../ReportsPage";

import { Chart as ChartJS, ChartData, registerables } from "chart.js";
import chartTrendline from "chartjs-plugin-trendline";
import { Section, useIdParams, useLayout } from "@andrewmclachlan/mooapp";

import { PeriodSelector } from "components/PeriodSelector";
import { InOutTrend } from "./InOutTrend";
import { InOut } from "./InOut";
import { Period } from "helpers/dateFns";

ChartJS.register(...registerables);
ChartJS.register(chartTrendline);

export const InOutPage = () => {

    const accountId = useIdParams();

    const [period, setPeriod] = useState<Period>({ startDate: null, endDate: null });

   return (
        <ReportsPage title="Income vs Expenses">
            <PeriodSelector onChange={setPeriod} />
            <Section title="Total Income vs Expenses" size={3} className="report inout">
                <InOut accountId={accountId} period={period} />
            </Section>
            <Section title="Income vs Expenses per Month" size={3} className="report inout-trend">
                <InOutTrend accountId={accountId} period={period} />
            </Section>
        </ReportsPage>
    );

}
