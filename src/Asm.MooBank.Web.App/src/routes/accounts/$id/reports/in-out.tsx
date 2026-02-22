import { useState } from "react";
import { createFileRoute } from "@tanstack/react-router";

import { ReportsPage } from "./-components/ReportsPage";
import "./-components/Reports.css";

import { useIdParams } from "@andrewmclachlan/moo-app";
import { Section } from "@andrewmclachlan/moo-ds";
import { Chart as ChartJS, registerables } from "chart.js";
import chartTrendline from "chartjs-plugin-trendline";

import { differenceInMonths } from "date-fns";

import { Period, subtractYear } from "helpers/dateFns";
import { InOut } from "./-components/InOut";
import { InOutTrend } from "./-components/InOutTrend";
import { useInOutAverageReport } from "../../-hooks/useInOutAverageReport";
import { useInOutReport } from "hooks/useInOutReport";
import { Col, Row } from "@andrewmclachlan/moo-ds";
import { MiniPeriodSelector } from "components/MiniPeriodSelector";
import { getPeriod } from "hooks";

ChartJS.register(...registerables);
ChartJS.register(chartTrendline);

export const Route = createFileRoute("/accounts/$id/reports/in-out")({
    component: InOutPage,
});

function InOutPage() {

    const accountId = useIdParams();

    const [period, setPeriod] = useState<Period>(getPeriod());

    const difference = Math.abs(differenceInMonths(period.startDate, period.endDate));
    const col = difference > 12 ? 12 : 6;

    return (
        <ReportsPage title="Income vs Expenses">
            <Section className="mini-filter-panel">
                <MiniPeriodSelector onChange={setPeriod} />
            </Section>
            <Row>
                <Col xxl={col} xl={12}>
                    <Section header="This period" headerSize={3} className="report inout">
                        <InOut accountId={accountId} period={period} useInOutReport={useInOutReport} />
                    </Section>
                </Col>
                <Col xxl={col} xl={12} hidden={difference > 12}>
                    <Section header="Same Period Last Year" headerSize={3} className="report inout">
                        <InOut accountId={accountId} period={subtractYear(period)} useInOutReport={useInOutReport} />
                    </Section>
                </Col>
            </Row>
            <Row hidden={difference <= 1} >
                <Col xxl={12}>
                    <Section header="Monthly Average" headerSize={3} className="report inout">
                        <InOut accountId={accountId} period={period} useInOutReport={useInOutAverageReport} />
                    </Section>
                </Col>
            </Row>
            <Section hidden={difference <= 1} header="Income vs Expenses per Month" headerSize={3} className="report inout-trend">
                <InOutTrend accountId={accountId} period={period} />
            </Section>
        </ReportsPage>
    );
}
