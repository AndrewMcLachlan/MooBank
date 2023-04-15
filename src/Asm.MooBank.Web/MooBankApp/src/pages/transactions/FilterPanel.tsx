import "./FilterPanel.scss";

import React, { useEffect, useState } from "react";
import { useDispatch, } from "react-redux";
import { FormRow as Row, TagSelector } from "components";
import { TransactionsSlice } from "store/Transactions";
import { Button, Col, Collapse, Form } from "react-bootstrap";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { PeriodSelector } from "components/PeriodSelector";
import { Period } from "helpers/dateFns";
import { Input } from "components/AccountList/Input";
import { useLocalStorage } from "@andrewmclachlan/mooapp";
import Select from "react-select";
import { useTags } from "services";
import { usePeriod } from "hooks";

export const FilterPanel: React.FC<FilterPanelProps> = () => {

    const [filterTagged, setFilterTagged] = useLocalStorage("filter-tagged", false);
    const [filterDescription, setFilterDescription] = useLocalStorage("filter-description", "");
    const [filterTag, setFilterTag] = useLocalStorage<number | null>("filter-tag", null);
    const [period, setPeriod] = useState<Period>({startDate: null,endDate: null});
    const tags = useTags();
    const dispatch = useDispatch();

    const [open, setOpen] = useState(false);

    const clear = () => {
        setFilterDescription("");
        setFilterTagged(false);
        setFilterTag(null);
    }

    const tag = tags.data?.find(t => t.id === filterTag);

    useEffect(() => {
        dispatch(TransactionsSlice.actions.setTransactionListFilter({ description: filterDescription, filterTagged, tag: filterTag, start: period.startDate?.toISOString(), end: period.endDate?.toISOString() }));
    }, [period, filterDescription, filterTagged, filterTag]);

    return (
        <fieldset className="filter-panel box">
            <legend className="clickable" onClick={() => setOpen(!open)}><FontAwesomeIcon icon={open ? "chevron-up" : "chevron-down"} size="xs" />Filters</legend>
            <div className="control-panel"><Button onClick={clear} variant="link">Clear Filters</Button></div>
            <Collapse in={open}>
                <>
                    <Row>
                        <Col className="description">
                            <Form.Label htmlFor="filter-desc">Description</Form.Label>
                            <Input id="filter-desc" type="text" value={filterDescription} onChange={(e) => setFilterDescription(e.currentTarget.value)} placeholder="Contains..." clearable />
                        </Col>
                        <Col>
                            <Form.Label htmlFor="filter-desc">Tags</Form.Label>
                            <Select options={tags.data ?? []} isClearable value={tag} getOptionLabel={t => t.name} getOptionValue={t => t.id.toString()} onChange={(v) => setFilterTag(v?.id ?? null)} className="react-select" classNamePrefix="react-select" />
                        </Col>
                    </Row>
                    <PeriodSelector instant onChange={setPeriod} />
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
