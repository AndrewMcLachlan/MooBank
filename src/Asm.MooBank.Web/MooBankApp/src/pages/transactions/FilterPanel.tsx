import "./FilterPanel.scss";

import React, { useEffect, useState } from "react";
import { useDispatch, } from "react-redux";
import { FormRow as Row } from "../../components";
import { TransactionsSlice } from "../../store/Transactions";
import { Col, Collapse, Form } from "react-bootstrap";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { PeriodSelector } from "../../components/PeriodSelector";
import { Period } from "../../helpers/dateFns";
import { getCachedPeriod } from "../../helpers";
import { Input } from "../../components/AccountList/Input";
import { useLocalStorage } from "@andrewmclachlan/mooapp";

export const FilterPanel: React.FC<FilterPanelProps> = () => {

    const [filterTagged, setFilterTagged] = useLocalStorage("filter-tagged", false);
    const [filterDescription, setFilterDescription] = useLocalStorage("filter-description", "");
    const [period, setPeriod] = useState<Period>(getCachedPeriod());
    const dispatch = useDispatch();

    const [open, setOpen] = useState(false);

    useEffect(() => {
        dispatch(TransactionsSlice.actions.setTransactionListFilter({description: filterDescription, filterTagged, start: period.startDate.toISOString(), end: period.endDate.toISOString() }));
    }, [period, filterDescription, filterTagged]);

    return (
        <fieldset className="filter-panel box">
            <legend className="clickable" onClick={() => setOpen(!open)}><FontAwesomeIcon icon={open ? "chevron-up" : "chevron-down"} size="xs" /> Filters</legend>
            <Collapse in={open}>
                <>
                <Row>
                    <Col className="description">
                        <Form.Label htmlFor="filter-desc">Description</Form.Label>
                        <Input id="filter-desc" type="text" value={filterDescription} onChange={(e) => setFilterDescription(e.currentTarget.value)} placeholder="Contains..." clearable />
                    </Col>
                </Row>
                <PeriodSelector value={period} onChange={setPeriod} instant />
                <Row>
                    <Form.Group>
                        <Form.Check id="filter-tagged" label="Only show transactions without tags" checked={filterTagged} onChange={(e) => setFilterTagged(e.currentTarget.checked)} />
                    </Form.Group>
                </Row>
                </>
            </Collapse>
        </fieldset>
    );
}

export interface FilterPanelProps {
}
