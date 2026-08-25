import React from "react";

import type { Bill, Account } from "api/types.gen";
import { Drawer } from "@andrewmclachlan/moo-ds";
import { Section } from "@andrewmclachlan/moo-ds";
import { getUnit } from "utils/units";
import { Amount } from "components";
import { formatDateShort, formatDateRange } from "utils/dateFns";

const formatNumber = (value: number | undefined, decimals = 2) => {
    if (value === undefined || value === null) return "-";
    return value.toFixed(decimals);
};

export const BillDetails: React.FC<BillDetailsProps> = ({ account, bill, show, onHide }) => {

    if (!bill) return null;

    return (
        <Drawer show={show} onHide={onHide} placement="end" className="bill-details">
            <Drawer.Header closeButton>

                {bill.invoiceNumber ? `Bill #${bill.invoiceNumber}` : "Bill Details"}

            </Drawer.Header>
            <Drawer.Body>
                <Section header="Summary">
                    <div className="summary-row">
                        <span className="summary-label">Account</span>
                        <span className="summary-value">{account?.name}</span>
                    </div>
                    <div className="summary-row">
                        <span className="summary-label">Issue Date</span>
                        <span>{formatDateShort(bill.issueDate)}</span>
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
                        <span className="summary-value-large"><Amount amount={bill.cost} currencyCode="AUD" /></span>
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
                                        {formatDateRange(period.periodStart, period.periodEnd)}
                                    </span>
                                    <span className="badge bg-secondary">{period.daysInclusive} days</span>
                                </div>
                                <div className="period-details">
                                    {period.usages?.map(usage => (
                                        <React.Fragment key={usage.usageType}>
                                            <div className="period-line">
                                                <span className="period-line-label">{usage.usageType === "Export" ? "Export (feed-in)" : "Usage"}</span>
                                                <span>{formatNumber(usage.totalUsage, 3)} {getUnit(account?.utilityType)} @ <Amount amount={usage.pricePerUnit} currencyCode="AUD" />/{getUnit(account?.utilityType)}</span>
                                            </div>
                                            <div className="period-line">
                                                <span></span>
                                                <span className="summary-value"><Amount amount={usage.cost} currencyCode="AUD" /></span>
                                            </div>
                                        </React.Fragment>
                                    ))}
                                    {period.serviceCharges?.map(charge => (
                                        <React.Fragment key={charge.chargeTypeId}>
                                            <div className="period-line">
                                                <span className="period-line-label">{charge.chargeTypeName ?? "Service Charge"}</span>
                                                <span>{period.daysInclusive} days @ <Amount amount={charge.chargePerDay} currencyCode="AUD" />/day</span>
                                            </div>
                                            <div className="period-line">
                                                <span></span>
                                                <span className="summary-value"><Amount amount={period.daysInclusive * charge.chargePerDay} currencyCode="AUD" /></span>
                                            </div>
                                        </React.Fragment>
                                    ))}
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
                                <Amount amount={bill.periods.reduce((sum, p) =>
                                    sum
                                    + p.usages.reduce((usage, u) => usage + (u.cost ?? 0), 0)
                                    + p.serviceCharges.reduce((charges, c) => charges + (p.daysInclusive * c.chargePerDay), 0), 0)} currencyCode="AUD" />
                            </span>
                        </div>
                    </Section>
                )}
            </Drawer.Body>
        </Drawer>
    );
};

export interface BillDetailsProps {
    account: Account;
    show: boolean;
    onHide: () => void;
    bill: Bill;
    onChange?: (bill: Bill) => void;
}
