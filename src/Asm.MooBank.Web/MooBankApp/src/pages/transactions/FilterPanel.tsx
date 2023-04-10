import "./FilterPanel.scss";

import React, { useEffect, useState } from "react";
import { useDispatch, useSelector } from "react-redux";
import { FormRow as Row } from "../../components";
import { TransactionsSlice } from "../../store/Transactions";
import { State } from "../../store/state";
import { Col, Collapse, Form } from "react-bootstrap";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { PeriodSelector } from "../../components/PeriodSelector";
import { Period } from "../../helpers/dateFns";
import { getCachedPeriod } from "../../helpers";

export const FilterPanel: React.FC<FilterPanelProps> = (props) => {

    const filterTagged = useSelector((state: State) => state.transactions.filter.filterTagged);
    const filterDescription = useSelector((state: State) => state.transactions.filter.description);
    const [period, setPeriod] = useState<Period>(getCachedPeriod());
    const filterStart = useSelector((state: State) => state.transactions.filter.start);
    const filterEnd = useSelector((state: State) => state.transactions.filter.end);
    const dispatch = useDispatch();

    const [open, setOpen] = useState(false);

    useEffect(() => {
        dispatch(TransactionsSlice.actions.setTransactionListFilter({ start: period.startDate.toISOString(), end: period.endDate.toISOString() }));
    }, [period]);

    return (
        <fieldset className="filter-panel box">
            <legend className="clickable" onClick={() => setOpen(!open)}><FontAwesomeIcon icon={open ? "chevron-up" : "chevron-down"} size="xs" /> Filters</legend>
            <Collapse in={open}>
                <>
                <Row>
                    <Col className="description">
                        <Form.Label htmlFor="filter-desc">Description</Form.Label>
                        <Form.Control id="filter-desc" type="text" value={filterDescription} onChange={(e) => dispatch(TransactionsSlice.actions.setTransactionListFilter({ description: e.currentTarget.value }))} placeholder="Contains..." />
                    </Col>
                </Row>
                <PeriodSelector value={period} onChange={setPeriod} instant />
                <Row>
                    <Form.Group>
                        <Form.Check id="filter-tagged" label="Only show transactions without tags" checked={filterTagged} onChange={(e) => dispatch(TransactionsSlice.actions.setTransactionListFilter({ filterTagged: e.currentTarget.checked }))} />
                    </Form.Group>
                </Row>
                </>
            </Collapse>
        </fieldset>
    );
}

export interface FilterPanelProps {
}
