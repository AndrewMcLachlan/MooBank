import React from "react";
import { Section } from "@andrewmclachlan/moo-ds";
import { useStockHolding } from "../../../-components/StockHoldingProvider"
import classNames from "classnames";
import { Amount } from "components/Amount";
import { useStockHoldingAdjustedGainLoss } from "routes/shares/-hooks/useStockHoldingAdjustedGainLoss";

export const StockSummary: React.FC<StockSummaryProps> = ({className, ...props}) => {

    const stockHolding = useStockHolding();
    const {data: adjustedGainLoss} = useStockHoldingAdjustedGainLoss(stockHolding?.id);

    if (!stockHolding) return null;

    return (
        <Section className={classNames("summary", className)} {...props} header={stockHolding.name}>
            <div className="key-value">
                <div>Balance</div>
                <div className="balance amount"><Amount amount={stockHolding.currentBalance} /></div>
            </div>
            <div className="key-value">
                <div>Quantity</div>
                <div className="amount">{stockHolding.quantity}</div>
            </div>
            <div className="key-value">
                <div>Current Price</div>
                <div>{stockHolding.currentPrice}</div>
            </div>
            <div className="key-value">
                <div>Capital Gain</div>
                <div><Amount amount={stockHolding.gainLoss} negativeColour plus minus /></div>
            </div>
            <div className="key-value">
                <div>Adjusted Gain</div>
                <div><Amount amount={adjustedGainLoss} negativeColour plus minus /></div>
            </div>
            <hr/>
            <div className="key-value">
                <div>Type</div>
                <div>Shares</div>
            </div>

        </Section>
    );
}

export interface StockSummaryProps extends React.HTMLAttributes<HTMLElement> {
}
