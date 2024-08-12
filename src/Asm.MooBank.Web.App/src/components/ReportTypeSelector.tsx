import React, { HTMLAttributes } from "react";
import { Button, ButtonGroup, Col, Row } from "react-bootstrap";
import { ReportType } from "models/reports";

export const ReportTypeSelector: React.FC<ReportTypeSelectorProps> = ({value, onChange, ...rest}) => {

    const onClick = (reportType: ReportType) => {
        if (value === reportType) return;
        onChange?.(reportType);
    }

    return (
        <Row {...rest}>
            <Col xl="4">
                <ButtonGroup>
                    <Button variant={value == ReportType.Income ? "primary" : "secondary"} onClick={() => onClick(ReportType.Income)}>Income</Button>
                    <Button variant={value == ReportType.Expenses ? "primary" : "secondary"} onClick={() => onClick(ReportType.Expenses)}>Expenses</Button>
                </ButtonGroup>
            </Col>
        </Row>
    );
}

export interface ReportTypeSelectorProps extends Omit<HTMLAttributes<HTMLDivElement>, "onChange"> {
    value?: ReportType;
    onChange?: (reportType: ReportType) => void;
}
