import React from "react";
import { Section } from "@andrewmclachlan/mooapp";
import { useStockHolding } from "../StockHoldingProvider"
import classNames from "classnames";
import { Amount } from "components/Amount";

export const StockSummary: React.FC<StockSummaryProps> = ({className, ...props}) => {

    const stockHolding = useStockHolding();

    if (!stockHolding) return null;

    return (
        <Section className={classNames("summary", className)} {...props} header={stockHolding.name}>
            <div className="key-value">
                <div>Balance</div>
                <div className="balance amount"><Amount amount={stockHolding.currentBalance} creditdebit /></div>
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
                <div><Amount amount={stockHolding.gainLoss} colour plusminus /></div>
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
