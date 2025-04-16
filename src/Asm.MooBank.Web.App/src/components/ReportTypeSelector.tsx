import React, { HTMLAttributes } from "react";
import { Button, ButtonGroup, Col, Row } from "react-bootstrap";
import { transactionTypeFilter } from "store/state";

export const ReportTypeSelector: React.FC<ReportTypeSelectorProps> = ({value, onChange, ...rest}) => {

    const onClick = (reportType: transactionTypeFilter) => {
        if (value === reportType) return;
        onChange?.(reportType);
    }

    return (
        <Row {...rest}>
            <Col xl="4">
                <ButtonGroup>
                    <Button variant={value == "Credit" ? "primary" : "secondary"} onClick={() => onClick("Credit")}>Income</Button>
                    <Button variant={value == "Debit" ? "primary" : "secondary"} onClick={() => onClick("Debit")}>Expense</Button>
                </ButtonGroup>
            </Col>
        </Row>
    );
}

export interface ReportTypeSelectorProps extends Omit<HTMLAttributes<HTMLDivElement>, "onChange"> {
    value?: transactionTypeFilter;
    onChange?: (reportType: transactionTypeFilter) => void;
}
