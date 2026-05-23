import React from "react";

import { Amount } from "components";

export interface PriceDeltaProps {
    current: number;
    previous: number | null | undefined;
    currencyCode?: string;
}

export const PriceDelta: React.FC<PriceDeltaProps> = ({ current, previous, currencyCode }) => {
    if (previous == null || previous === 0) return null;
    const diff = current - previous;
    if (diff === 0) return null;
    const pct = (diff / previous) * 100;
    const direction = diff > 0 ? "up" : "down";

    return (
        <span className={`price-delta price-delta-${direction}`} title="Change since previous recorded price">
            <span className="arrow" aria-hidden="true">{direction === "up" ? "▲" : "▼"}</span>
            <Amount amount={diff} currencyCode={currencyCode} plus minus />
            <span className="pct">({diff > 0 ? "+" : "−"}{Math.abs(pct).toFixed(1)}%)</span>
        </span>
    );
};

PriceDelta.displayName = "PriceDelta";
