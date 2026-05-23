import React from "react";
import { Badge, type BadgeHue } from "@andrewmclachlan/moo-ds";

const hueByType: Record<string, BadgeHue> = {
    Transaction: "blue",
    Savings: "emerald",
    Credit: "orange",
    Mortgage: "rose",
    Loan: "pink",
    Superannuation: "indigo",
    Investment: "teal",
    Broker: "purple",
    Asset: "amber",
    Shares: "cyan",
    Virtual: "slate",
    "Reserved Sum": "neutral",
};

export interface AccountTypeBadgeProps {
    type: string | null | undefined;
}

export const AccountTypeBadge: React.FC<AccountTypeBadgeProps> = ({ type }) => {
    if (!type) return null;
    const hue = hueByType[type];
    if (hue) {
        return <Badge bg={hue} muted pill>{type}</Badge>;
    }
    return <Badge bg="secondary" muted pill>{type}</Badge>;
};

AccountTypeBadge.displayName = "AccountTypeBadge";
