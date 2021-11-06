import "./FilterPanel.scss";

import React, { useState } from "react";
import { useDispatch, useSelector } from "react-redux";

import { TransactionsSlice } from "../../store/Transactions";
import { State } from "../../store/state";
import { Collapse, Form, Row } from "react-bootstrap";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";

export const FilterPanel: React.FC<FilterPanelProps> = (props) => {

    const filterTagged = useSelector((state: State) => state.transactions.filter.filterTagged);
    const filterDescription = useSelector((state: State) => state.transactions.filter.description);
    const filterStart = useSelector((state: State) => state.transactions.filter.start);
    const filterEnd = useSelector((state: State) => state.transactions.filter.end);
    const dispatch = useDispatch();

    const [open, setOpen] = useState(false);

    return (
            <fieldset className="filter-panel box">
                <legend className="clickable" onClick={() => setOpen(!open)}><FontAwesomeIcon icon={open ? "chevron-up" : "chevron-down"} size="xs" /> Filters</legend>
                <Collapse in={open}>
                <Row>
                    <Form.Group className="description">
                        <Form.Label htmlFor="filter-desc">Description</Form.Label>
                        <Form.Control id="filter-desc" type="text" value={filterDescription} onChange={(e) => dispatch(TransactionsSlice.actions.setTransactionListFilter({ description: e.currentTarget.value }))} placeholder="Contains..." />
                    </Form.Group>
                    <Form.Group>
                        <Form.Label htmlFor="filter-start">From</Form.Label>
                        <Form.Control id="filter-start" type="date" value={filterStart} onChange={(e) => dispatch(TransactionsSlice.actions.setTransactionListFilter({ start: e.currentTarget.value }))} />
                    </Form.Group>
                    <Form.Group>
                        <Form.Label htmlFor="filter-end">To</Form.Label>
                        <Form.Control id="filter-end" type="date" value={filterEnd} onChange={(e) => dispatch(TransactionsSlice.actions.setTransactionListFilter({ end: e.currentTarget.value }))} />
                    </Form.Group>
                    <Form.Group>
                        <Form.Check id="filter-tagged" label="Only show transactions without tags" checked={filterTagged} onChange={(e) => dispatch(TransactionsSlice.actions.setTransactionListFilter({ filterTagged: e.currentTarget.checked }))} />
                    </Form.Group>
                </Row>
                </Collapse>
            </fieldset>
    );
}

export interface FilterPanelProps {
}
