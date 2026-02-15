import React, { HTMLAttributes } from "react";
import { Button, ButtonGroup, Col, Row } from "@andrewmclachlan/moo-ds";
import { transactionTypeFilter } from "store/state";

export const ReportTypeSelector: React.FC<ReportTypeSelectorProps> = ({ value, onChange, ...rest }) => {

    const onClick = (reportType: transactionTypeFilter) => {
        if (value === reportType) return;
        onChange?.(reportType);
    }

    return (
        <ButtonGroup className="btn-group-form" aria-label="Filter by income or expense">
            <Button variant={value == "Credit" ? "primary" : "outline-primary"} onClick={() => onClick("Credit")}>Income</Button>
            <Button variant={value == "Debit" ? "primary" : "outline-primary"} onClick={() => onClick("Debit")}>Expense</Button>
        </ButtonGroup>
    );
}

export interface ReportTypeSelectorProps extends Omit<HTMLAttributes<HTMLDivElement>, "onChange"> {
    value?: transactionTypeFilter;
    onChange?: (reportType: transactionTypeFilter) => void;
}
