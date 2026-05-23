import React from "react";
import { Section } from "@andrewmclachlan/moo-ds";

import { Amount } from "components";
import { formatDate } from "utils/dateFns";

import { useStockHolding } from "../../../-components/StockHoldingProvider";
import { useStockHoldingAdjustedGainLoss } from "routes/shares/-hooks/useStockHoldingAdjustedGainLoss";

import { usePeriodLabel } from "../-hooks/usePeriodLabel";
import { useStockTransactionTotal } from "../-hooks/useStockTransactionTotal";
import { PriceDelta } from "./PriceDelta";

export const StockHoldingCard: React.FC = () => {

    const stockHolding = useStockHolding();
    const periodLabel = usePeriodLabel();
    const total = useStockTransactionTotal(stockHolding?.id ?? "");
    const { data: adjustedGainLoss } = useStockHoldingAdjustedGainLoss(stockHolding?.id);

    if (!stockHolding) return null;

    const balance = stockHolding.currentBalanceLocalCurrency ?? stockHolding.currentBalance ?? 0;
    const currency = stockHolding.currency;
    const instrumentType = stockHolding.instrumentType ?? "Shares";

    return (
        <Section className="tx-account-card accent-stripe">
            <div className="tx-card-grid">
                <div className="hero-block">
                    <div className="eyebrow">{stockHolding.name} · Balance</div>
                    <div className="hero-balance"><Amount amount={balance} currencyCode={currency} minus /></div>
                    <div className="hero-subline">
                        Last update <span className="strong">{formatDate(stockHolding.balanceDate)}</span>
                        <span className="sep">·</span>
                        Type <span className="strong">{instrumentType}</span>
                    </div>
                </div>

                <div className="period-block">
                    <div className="period-head">
                        <span className="eyebrow">{periodLabel}</span>
                        <span className="period-count">{total} {total === 1 ? "transaction" : "transactions"}</span>
                    </div>

                    <div className="stat-grid stat-grid-4">
                        <div className="stat-block">
                            <div className="lbl">Quantity</div>
                            <div className="val">{stockHolding.quantity.toLocaleString()}</div>
                        </div>
                        <div className="stat-block">
                            <div className="lbl">Current Price</div>
                            <div className="val"><Amount amount={stockHolding.currentPrice} currencyCode={currency} /></div>
                            {stockHolding.previousPrice != null && (
                                <div className="stat-delta">
                                    <PriceDelta current={stockHolding.currentPrice} previous={stockHolding.previousPrice} currencyCode={currency} />
                                </div>
                            )}
                        </div>
                        <div className="stat-block">
                            <div className="lbl">Capital Gain</div>
                            <div className="val"><Amount amount={stockHolding.gainLoss} currencyCode={currency} plus minus positiveColour negativeColour /></div>
                        </div>
                        <div className="stat-block">
                            <div className="lbl">Adjusted Gain</div>
                            <div className="val"><Amount amount={adjustedGainLoss ?? 0} currencyCode={currency} plus minus positiveColour negativeColour /></div>
                        </div>
                    </div>
                </div>
            </div>
        </Section>
    );
};

StockHoldingCard.displayName = "StockHoldingCard";
