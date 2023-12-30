import React from "react";
import { Section } from "@andrewmclachlan/mooapp";
import { useStockHolding } from "../StockHoldingProvider"
import classNames from "classnames";
import { AccountBalance } from "components";

export const StockSummary: React.FC<StockSummaryProps> = ({className, ...props}) => {

    const stockHolding = useStockHolding();

    if (!stockHolding) return null;

    return (
        <Section className={classNames("summary", className)} {...props} title={stockHolding.name}>
            <div className="key-value">
                <div>Balance</div>
                <div className="balance amount"><AccountBalance balance={stockHolding.currentBalance} /></div>
            </div>
            <div className="key-value">
                <div>Current Price</div>
                <div>{stockHolding.currentPrice}</div>
            </div><hr/>
            <div className="key-value">
                <div>Type</div>
                <div>Stock Holding</div>
            </div>

        </Section>
    );
}

export interface StockSummaryProps extends React.HTMLAttributes<HTMLElement> {
}