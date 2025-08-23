import { Section, Tooltip } from "@andrewmclachlan/moo-ds";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import React, { useEffect } from "react";
import { Button, ButtonGroup, Col, Form } from "react-bootstrap";
import { useDispatch, } from "react-redux";

import { PeriodSelector, FormRow as Row, TagSelector } from "components";
import { TransactionsSlice } from "store/Transactions";
import { useFilterPanel } from "../hooks/useFilterPanel";

export const FilterPanel: React.FC<FilterPanelProps> = (props) => {

    const { filterDescription, filterTagged, filterTags, filterNetZero, filterType, storedFilterType, period, clear, setFilterDescription, setFilterTagged, setFilterNetZero, setFilterTags, setFilterType, setPeriod } = useFilterPanel();

    const dispatch = useDispatch();

    useEffect(() => {
        dispatch(TransactionsSlice.actions.setTransactionListFilter({ description: filterDescription, filterTagged, filterNetZero, tags: filterTags, transactionType: storedFilterType, start: period?.startDate?.toISOString(), end: period?.endDate?.toISOString() }));
    }, [period, filterDescription, filterTagged, filterNetZero, filterTags, storedFilterType, window.location.search]);

    return (
        <Section className="filter-panel" header="Filter" {...props}>
            <div className="control-panel"><FontAwesomeIcon className="clickable" title="Clear filters" icon="filter-circle-xmark" onClick={clear} size="lg" aria-controls="filter-panel-collapse" /></div>
            <Row>
                <Col className="description" lg={4} xl={5}>
                    <Form.Label htmlFor="filter-desc">Description</Form.Label><Tooltip id="filter-desc">Search for multiple terms by separating them with a comma</Tooltip>
                    <Form.Control id="filter-desc" type="search" value={filterDescription} onChange={(e) => setFilterDescription(e.currentTarget.value)} placeholder="Contains..." />
                </Col>
                <Col lg={4}>
                    <Form.Label htmlFor="filter-tags">Tags</Form.Label>
                    <TagSelector id="filter-tags" onChange={setFilterTags} multiSelect value={filterTags} />
                </Col>
                <Col lg={4} xl={3}>
                    <Form.Label htmlFor="filter-type">Type</Form.Label>
                    <ButtonGroup className="btn-group-form" aria-label="Filter by income or expense" id="filter-type">
                        <Button id="filter-all" variant={filterType === "" ? "primary" : "outline-primary"} onClick={() => setFilterType("")}>All</Button>
                        <Button id="filter-income" variant={filterType === "Credit" ? "primary" : "outline-primary"} onClick={() => setFilterType("Credit")}>Income</Button>
                        <Button id="filter-expense" variant={filterType === "Debit" ? "primary" : "outline-primary"} onClick={() => setFilterType("Debit")}>Expense</Button>
                    </ButtonGroup>
                </Col>
            </Row>
            <PeriodSelector instant onChange={setPeriod} />
            <Row>
                <Form.Group as={Col} lg={6} xl={4}>
                    <Form.Switch id="filter-tagged" label="Only show transactions without tags" checked={filterTagged} onChange={(e) => setFilterTagged(e.currentTarget.checked)} />
                </Form.Group>
                <Form.Group as={Col} lg={6} xl={4}>
                    <Form.Switch id="filter-netzero" label="Exclude fully offset transactions" checked={filterNetZero} onChange={(e) => setFilterNetZero(e.currentTarget.checked)} />
                </Form.Group>
            </Row>
        </Section>
    );
};

FilterPanel.displayName = "FilterPanel";

export interface FilterPanelProps extends React.HTMLAttributes<HTMLElement> {
}
