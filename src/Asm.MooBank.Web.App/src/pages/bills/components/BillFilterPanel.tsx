import React from "react";
import { Form } from "react-bootstrap";
import { Section } from "@andrewmclachlan/moo-ds";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";

import { BillAccount } from "models/bills";
import { BillFilter } from "services/BillService";

export interface BillFilterPanelProps {
    accounts?: BillAccount[];
    filter: BillFilter;
    onFilterChange: (filter: BillFilter) => void;
}

export const BillFilterPanel: React.FC<BillFilterPanelProps> = ({ accounts, filter, onFilterChange }) => {

    const handleStartDateChange = (value: string) => {
        onFilterChange({ ...filter, startDate: value || undefined });
    };

    const handleEndDateChange = (value: string) => {
        onFilterChange({ ...filter, endDate: value || undefined });
    };

    const handleAccountChange = (value: string) => {
        onFilterChange({ ...filter, accountId: value || undefined });
    };

    const clearFilter = () => {
        onFilterChange({});
    };

    return (
        <Section className="bill-filter-panel" header="Filter">
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
                    <Form.Label htmlFor="filter-start">From</Form.Label>
                    <Form.Control
                        id="filter-start"
                        type="date"
                        value={filter.startDate ?? ""}
                        onChange={(e) => handleStartDateChange(e.target.value)}
                    />
                </div>
                <div className="filter-field">
                    <Form.Label htmlFor="filter-end">To</Form.Label>
                    <Form.Control
                        id="filter-end"
                        type="date"
                        value={filter.endDate ?? ""}
                        onChange={(e) => handleEndDateChange(e.target.value)}
                    />
                </div>
                {accounts && accounts.length > 0 && (
                    <div className="filter-field">
                        <Form.Label htmlFor="filter-account">Account</Form.Label>
                        <Form.Select
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
                        </Form.Select>
                    </div>
                )}
            </div>
        </Section>
    );
};

BillFilterPanel.displayName = "BillFilterPanel";
