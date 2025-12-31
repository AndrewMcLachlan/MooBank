import { format, parseISO } from "date-fns";
import { Bill, BillAccount } from "models/bills";
import { Offcanvas } from "react-bootstrap";
import { Section } from "@andrewmclachlan/moo-ds";
import { getUnit } from "helpers";

const formatCurrency = (value: number | undefined) => {
    if (value === undefined || value === null) return "-";
    return new Intl.NumberFormat("en-AU", { style: "currency", currency: "AUD" }).format(value);
};

const formatNumber = (value: number | undefined, decimals = 2) => {
    if (value === undefined || value === null) return "-";
    return value.toFixed(decimals);
};

export const BillDetails: React.FC<BillDetailsProps> = ({ account, bill, show, onHide }) => {

    if (!bill) return null;

    return (
        <Offcanvas show={show} onHide={onHide} placement="end" className="bill-details">
            <Offcanvas.Header closeButton>
                <Offcanvas.Title>
                    {bill.invoiceNumber ? `Bill #${bill.invoiceNumber}` : "Bill Details"}
                </Offcanvas.Title>
            </Offcanvas.Header>
            <Offcanvas.Body>
                <Section header="Summary">
                    <div className="summary-row">
                        <span className="summary-label">Account</span>
                        <span className="summary-value">{account?.name}</span>
                    </div>
                    <div className="summary-row">
                        <span className="summary-label">Issue Date</span>
                        <span>{format(parseISO(bill.issueDate), "dd MMM yyyy")}</span>
                    </div>
                    {bill.previousReading !== undefined && bill.currentReading !== undefined && (
                        <div className="summary-row">
                            <span className="summary-label">Readings</span>
                            <span>{bill.previousReading} → {bill.currentReading}</span>
                        </div>
                    )}
                    {bill.total > 0 && (
                        <div className="summary-row">
                            <span className="summary-label">Total Usage</span>
                            <span>{formatNumber(bill.total, 0)} {getUnit(account?.utilityType)}</span>
                        </div>
                    )}
                    <div className="summary-row">
                        <span className="summary-label">Total Cost</span>
                        <span className="summary-value-large">{formatCurrency(bill.cost)}</span>
                    </div>
                    {bill.costsIncludeGST && (
                        <div className="gst-note">Includes GST</div>
                    )}
                </Section>

                {bill.periods && bill.periods.length > 0 && (
                    <Section header="Billing Periods">
                        {bill.periods.map((period, index) => (
                            <div key={index} className="period-item">
                                <div className="period-header">
                                    <span className="period-dates">
                                        {format(parseISO(period.periodStart), "dd MMM")} - {format(parseISO(period.periodEnd), "dd MMM yyyy")}
                                    </span>
                                    <span className="badge bg-secondary">{period.daysInclusive} days</span>
                                </div>
                                <div className="period-details">
                                    <div className="period-line">
                                        <span className="period-line-label">Usage</span>
                                        <span>{formatNumber(period.totalUsage, 3)} {getUnit(account?.utilityType)} @ {formatCurrency(period.pricePerUnit)}/{getUnit(account?.utilityType)}</span>
                                    </div>
                                    <div className="period-line">
                                        <span></span>
                                        <span className="summary-value">{formatCurrency(period.cost)}</span>
                                    </div>
                                    <div className="period-line">
                                        <span className="period-line-label">Service Charge</span>
                                        <span>{period.days} days @ {formatCurrency(period.chargePerDay)}/day</span>
                                    </div>
                                    <div className="period-line">
                                        <span></span>
                                        <span className="summary-value">{formatCurrency(period.days * period.chargePerDay)}</span>
                                    </div>
                                </div>
                            </div>
                        ))}
                    </Section>
                )}

                {bill.periods && bill.periods.length > 0 && (
                    <Section>
                        <div className="period-total">
                            <span>Period Total</span>
                            <span>
                                {formatCurrency(
                                    bill.periods.reduce((sum, p) => sum + p.cost + (p.days * p.chargePerDay), 0)
                                )}
                            </span>
                        </div>
                    </Section>
                )}
            </Offcanvas.Body>
        </Offcanvas>
    );
};

export interface BillDetailsProps {
    account: BillAccount;
    show: boolean;
    onHide: () => void;
    bill: Bill;
    onChange?: (bill: Bill) => void;
}
