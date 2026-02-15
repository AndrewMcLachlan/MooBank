import { Section } from "@andrewmclachlan/moo-ds";
import { format, parseISO } from "date-fns";
import { Col, Row } from "@andrewmclachlan/moo-ds";
import { ForecastSummary } from "models";

interface ForecastSummaryPanelProps {
    summary: ForecastSummary;
}

export const ForecastSummaryPanel: React.FC<ForecastSummaryPanelProps> = ({ summary }) => {
    const lowestBalanceDate = summary && parseISO(summary?.lowestBalanceMonth);

    return (
        <Section header="Summary">
            <Row className="forecast-summary">
                <Col md={4}>
                    <div className="summary-card">
                        <div className="summary-label">Lowest Balance</div>
                        <div className={`summary-value ${summary?.lowestBalance < 0 ? 'negative' : ''}`}>
                            ${summary?.lowestBalance.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 })}
                        </div>
                        <div className="summary-sublabel">
                           {summary && `in ${format(lowestBalanceDate, "MMMM yyyy")}`}
                        </div>
                    </div>
                </Col>
                <Col md={4}>
                    <div className="summary-card">
                        <div className="summary-label">Required Monthly Uplift</div>
                        <div className={`summary-value ${summary?.requiredMonthlyUplift > 0 ? 'negative' : ''}`}>
                            ${summary?.requiredMonthlyUplift.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 })}
                        </div>
                        <div className="summary-sublabel">
                            {summary?.requiredMonthlyUplift > 0 ? 'to avoid negative balance' : 'no uplift required'}
                        </div>
                    </div>
                </Col>
                <Col md={4}>
                    <div className="summary-card">
                        <div className="summary-label">Months Below Zero</div>
                        <div className={`summary-value ${summary?.monthsBelowZero > 0 ? 'negative' : ''}`}>
                            {summary?.monthsBelowZero}
                        </div>
                        <div className="summary-sublabel">
                            {summary?.monthsBelowZero === 0 ? 'looking good!' : 'needs attention'}
                        </div>
                    </div>
                </Col>
            </Row>
            <Row className="forecast-summary mt-3">
                <Col md={6}>
                    <div className="summary-card">
                        <div className="summary-label">Total Projected Income</div>
                        <div className="summary-value">
                            ${summary?.totalIncome.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 })}
                        </div>
                    </div>
                </Col>
                <Col md={6}>
                    <div className="summary-card">
                        <div className="summary-label">Total Projected Outgoings</div>
                        <div className="summary-value">
                            ${summary?.totalOutgoings.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 })}
                        </div>
                    </div>
                </Col>
            </Row>
        </Section>
    );
};
