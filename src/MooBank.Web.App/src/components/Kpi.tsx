import type { PropsWithChildren } from "react";
import classNames from "classnames";
import { Section } from "@andrewmclachlan/moo-ds";

/**
 * Which side of the ledger a figure falls on, shown as the card's top accent.
 * Anything without a side — a balance, a count — takes the default accent.
 */
export type KpiTone = "income" | "expense" | "neutral";

/**
 * A headline figure: an uppercase label, the number, and an optional caption, on a card with a
 * coloured top edge.
 *
 * Four features had grown their own copy of this — transactions, forecast, retirement and budget —
 * with four sets of class names for the same three elements. The differences that remain are
 * genuine (each strip sizes its own type), so those stay in the feature stylesheets, scoped by the
 * grid class around the cards.
 */
export const Kpi: React.FC<PropsWithChildren<KpiProps>> = ({ label, tone = "neutral", className, children, ...props }) => (
    <Section className={classNames("kpi", className)} data-tone={tone} {...props}>
        <div className="eyebrow">{label}</div>
        {children}
    </Section>
);

export interface KpiProps extends Omit<React.HTMLAttributes<HTMLElement>, "children"> {
    label: React.ReactNode;
    tone?: KpiTone;
}

/** The figure itself. */
export const KpiValue: React.FC<PropsWithChildren<KpiValueProps>> = ({ className, children, ...props }) => (
    <div className={classNames("kpi-value", className)} {...props}>{children}</div>
);

export type KpiValueProps = React.HTMLAttributes<HTMLDivElement>;

/** The caption under the figure — what it covers, when it falls, why it matters. */
export const KpiSub: React.FC<PropsWithChildren<KpiSubProps>> = ({ className, children, ...props }) => (
    <div className={classNames("kpi-sub", className)} {...props}>{children}</div>
);

export type KpiSubProps = React.HTMLAttributes<HTMLDivElement>;
