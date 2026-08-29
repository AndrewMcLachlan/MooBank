import React, { useState } from "react";
import { Form, Input } from "@andrewmclachlan/moo-ds";
import { Section } from "@andrewmclachlan/moo-ds";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { parseISO, subYears } from "date-fns";

import type { Account } from "api/types.gen";
import { DayRangeSelector } from "components/DayRangeSelector";
import type { Period } from "models/dateFns";
import { formatISODate } from "utils/dateFns";
import type { BillFilter } from "../-hooks/types";

export interface BillFilterPanelProps {
    accounts?: Account[];
    filter: BillFilter;
    onFilterChange: (filter: BillFilter) => void;
}

export const BillFilterPanel: React.FC<BillFilterPanelProps> = ({ accounts, filter, onFilterChange }) => {

    const handleDatesChange = (period: Period) => {
        onFilterChange({ ...filter, startDate: formatISODate(period.startDate), endDate: formatISODate(period.endDate) });
    };

    const handleAccountChange = (value: string) => {
        onFilterChange({ ...filter, accountId: value || undefined });
    };

    /* Clears everything except the dates, as the transaction filters do: a list with no period at
       all is rarely what anyone wants back, and the range is the one filter you set deliberately. */
    const clearFilter = () => {
        onFilterChange({ startDate: filter.startDate, endDate: filter.endDate });
    };

    // Only reached by a filter stored before this control existed; read once rather than on every
    // render, which would also be a new Date on each pass.
    const [fallback] = useState<Period>(() => ({ startDate: subYears(new Date(), 2), endDate: new Date() }));

    const dates: Period = {
        startDate: filter.startDate ? parseISO(filter.startDate) : fallback.startDate,
        endDate: filter.endDate ? parseISO(filter.endDate) : fallback.endDate,
    };

    return (
        <Section className="bill-filter-panel">
            <div className="control-panel">
                <FontAwesomeIcon
                    className="clickable"
                    title="Clear filters"
                    icon="filter-circle-xmark"
                    onClick={clearFilter}
                    size="lg"
                />
            </div>
            <div className="filter-row">
                <div className="filter-field">
                    <Form.Label htmlFor="filter-dates">Dates</Form.Label>
                    <DayRangeSelector id="filter-dates" value={dates} onChange={handleDatesChange} />
                </div>
                {accounts && accounts.length > 0 && (
                    <div className="filter-field">
                        <Form.Label htmlFor="filter-account">Account</Form.Label>
                        <Input.Select
                            id="filter-account"
                            value={filter.accountId ?? ""}
                            onChange={(e) => handleAccountChange(e.target.value)}
                        >
                            <option value="">All Accounts</option>
                            {accounts.map((account) => (
                                <option key={account.id} value={account.id}>
                                    {account.name}
                                </option>
                            ))}
                        </Input.Select>
                    </div>
                )}
            </div>
        </Section>
    );
};

BillFilterPanel.displayName = "BillFilterPanel";
