import { useState } from "react";

import { ReportsPage } from "../ReportsPage";

import { Section, useIdParams } from "@andrewmclachlan/mooapp";
import { Chart as ChartJS, registerables } from "chart.js";
import chartTrendline from "chartjs-plugin-trendline";

import { differenceInMonths } from "date-fns";

import { PeriodSelector } from "components/PeriodSelector";
import { Period } from "helpers/dateFns";
import { InOut } from "./InOut";
import { InOutTrend } from "./InOutTrend";
import { useInOutAverageReport, useInOutReport } from "services";
import { Col, Row } from "react-bootstrap";

ChartJS.register(...registerables);
ChartJS.register(chartTrendline);

export const InOutPage = () => {

    const accountId = useIdParams();

    const [period, setPeriod] = useState<Period>({ startDate: null, endDate: null });

    const difference = Math.abs(differenceInMonths(period.startDate, period.endDate));
    const col = difference <= 1 ? 12 : 6;

    return (
        <ReportsPage title="Income vs Expenses">
            <Section>
                <PeriodSelector onChange={setPeriod} />
            </Section>
            <Row>
                <Col xxl={col} xl={12}>
                    <Section title="Total Income vs Expenses" titleSize={3} className="report inout">
                        <InOut accountId={accountId} period={period} useInOutReport={useInOutReport} />
                    </Section>
                </Col>
                <Col hidden={difference <= 1} xxl={col} xl={12}>
                    <Section title="Average Income vs Expenses" titleSize={3} className="report inout">
                        <InOut accountId={accountId} period={period} useInOutReport={useInOutAverageReport} />
                    </Section>
                </Col>
            </Row>
            <Section hidden={difference <= 1} title="Income vs Expenses per Month" titleSize={3} className="report inout-trend">
                <InOutTrend accountId={accountId} period={period} />
            </Section>
        </ReportsPage>
    );

}
