import "./FilterPanel.scss";

import { useLocalStorage } from "@andrewmclachlan/mooapp";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import classnames from "classnames";
import { useEffect, useState } from "react";
import { Col, Collapse, Form } from "react-bootstrap";
import { useDispatch, } from "react-redux";

import { FormRow as Row, TagSelector } from "components";
import { Input } from "components/AccountList/Input";
import { PeriodSelector } from "components";
import { TransactionsSlice } from "store/Transactions";

import { Period } from "helpers/dateFns";

export const FilterPanel: React.FC<FilterPanelProps> = () => {

    const [filterTagged, setFilterTagged] = useLocalStorage("filter-tagged", false);
    const [filterDescription, setFilterDescription] = useLocalStorage("filter-description", "");
    const [filterTag, setFilterTag] = useLocalStorage<number | null>("filter-tag", null);
    const [period, setPeriod] = useState<Period>({startDate: null,endDate: null});
    const dispatch = useDispatch();

    const [open, setOpen] = useLocalStorage("filter-panel-open", false);

    const clear = () => {
        setFilterDescription("");
        setFilterTagged(false);
        setFilterTag(null);
    }

    useEffect(() => {
        dispatch(TransactionsSlice.actions.setTransactionListFilter({ description: filterDescription, filterTagged, tag: filterTag, start: period?.startDate?.toISOString(), end: period?.endDate?.toISOString() }));
    }, [period, filterDescription, filterTagged, filterTag]);

    return (
        <fieldset className="filter-panel box">
            <legend className={classnames("clickable", open && "open")} onClick={() => setOpen(!open)}><FontAwesomeIcon icon="chevron-down" size="xs" />Filters</legend>
            <div className="control-panel"><FontAwesomeIcon className="clickable" title="Clear filters" icon="filter-circle-xmark" onClick={clear} size="lg" aria-controls="filter-panel-collapse" /></div>
            <Collapse in={open}>
                <div id="filter-panel-collapse">
                    <Row>
                        <Col className="description">
                            <Form.Label htmlFor="filter-desc">Description</Form.Label>
                            <Input id="filter-desc" type="text" value={filterDescription} onChange={(e) => setFilterDescription(e.currentTarget.value)} placeholder="Contains..." clearable />
                        </Col>
                        <Col>
                            <Form.Label htmlFor="filter-desc">Tags</Form.Label>
                            <TagSelector onChange={setFilterTag} value={filterTag} />
                        </Col>
                    </Row>
                    <PeriodSelector instant onChange={setPeriod} />
                    <Row>
                        <Form.Group>
                            <Form.Check id="filter-tagged" label="Only show transactions without tags" checked={filterTagged} onChange={(e) => setFilterTagged(e.currentTarget.checked)} />
                        </Form.Group>
                    </Row>
                </div>
            </Collapse>
        </fieldset>
    );
}

export interface FilterPanelProps {
}
