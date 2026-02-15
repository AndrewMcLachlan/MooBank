import { Section, Tooltip, useLocalStorage } from "@andrewmclachlan/moo-ds";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { useEffect, useState } from "react";
import { Col, Form, Input } from "@andrewmclachlan/moo-ds";
import { useDispatch, } from "react-redux";

import { PeriodSelector, FormRow as Row } from "components";
import { StockTransactionsSlice } from "store";

import { Period } from "helpers/dateFns";

export const FilterPanel: React.FC<FilterPanelProps> = (props) => {

    const [filterDescription, setFilterDescription] = useLocalStorage("filter-description", "");

     const [period, setPeriod] = useState<Period>({ startDate: null, endDate: null });
    const dispatch = useDispatch();

    const clear = () => {
        setFilterDescription("");
    }

    useEffect(() => {
        dispatch(StockTransactionsSlice.actions.setTransactionListFilter({ description: filterDescription, transactionType: "", start: period?.startDate?.toISOString(), end: period?.endDate?.toISOString() }));
    }, [period, filterDescription, window.location.search]);

    return (
        <Section className="filter-panel" header="Filter" {...props}>
            <div className="control-panel"><FontAwesomeIcon className="clickable" title="Clear filters" icon="filter-circle-xmark" onClick={clear} size="lg" aria-controls="filter-panel-collapse" /></div>
            <Row>
                <Col className="description">
                    <Form.Label htmlFor="filter-desc">Description</Form.Label><Tooltip id="filter-desc">Search for multiple terms by separating them with a comma</Tooltip>
                    <Input id="filter-desc" type="search" value={filterDescription} onChange={(e) => setFilterDescription(e.currentTarget.value)} placeholder="Contains..." />
                </Col>
            </Row>
            <PeriodSelector instant onChange={setPeriod} />
        </Section>
    );
}

export interface FilterPanelProps extends React.HTMLAttributes<HTMLElement> {
}
