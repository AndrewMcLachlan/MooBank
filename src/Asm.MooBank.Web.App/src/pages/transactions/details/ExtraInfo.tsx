import type { Transaction } from "api/types.gen";
import React from "react";

export const ExtraInfo: React.FC<ExtraInfoProps> = ({ transaction }) => {

    if (!transaction?.extraInfo) return null;

    const extraInfo = transaction.extraInfo as Record<string, string>;

    return (
        <section className="transaction-details-extra">
            {Object.keys(extraInfo).map(key =>
                <React.Fragment key={key}>
                    <label>{key.replace(/([A-Z])/g, " $1").trim()}</label>
                    <div className="value">{extraInfo[key]}</div>
                </React.Fragment>
            )}
        </section>
    );
}

export interface ExtraInfoProps {
    transaction: Transaction;
}