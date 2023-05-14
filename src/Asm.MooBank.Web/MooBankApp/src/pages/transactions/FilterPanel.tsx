import { Input, Tooltip, useLocalStorage } from "@andrewmclachlan/mooapp";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import classnames from "classnames";
import { useEffect, useState } from "react";
import { Col, Collapse, Form } from "react-bootstrap";
import { useDispatch, } from "react-redux";

import { FormRow as Row, TagSelector } from "components";
import { PeriodSelector } from "components";
import { TransactionsSlice } from "store/Transactions";

import { Period } from "helpers/dateFns";

export const FilterPanel: React.FC<FilterPanelProps> = () => {

    const params = new URLSearchParams(window.location.search);

    const [storedFilterTagged, setStoredFilterTagged] = useLocalStorage("filter-tagged", false);
    const [filterDescription, setFilterDescription] = useLocalStorage("filter-description", "");
    const [storedFilterTag, setStoredFilterTag] = useLocalStorage<number | null>("filter-tag", null);

    const [localFilterTag, setLocalFilterTag] = useState(Number(params.get("tag")) ?? storedFilterTag);
    const [localFilterTagged, setLocalFilterTagged] = useState<boolean>(Number(params.get("tag")) ? false : params.get("untagged") ? true : storedFilterTagged);
    const filterTag = localFilterTag ?? storedFilterTag;
    const filterTagged = localFilterTagged ?? storedFilterTagged;

    const setFilterTag = (tag: number) => {
        params.delete("tag");
        const queryString = params.toString();
        const newUrl = window.location.origin + window.location.pathname + (queryString === "" ? "" : `?${queryString}`);

        window.history.replaceState({ path: newUrl }, "", newUrl);

        setLocalFilterTag(tag);
        setStoredFilterTag(tag);
    }

    const setFilterTagged = (filter: boolean) => {
        params.delete("untagged");
        const queryString = params.toString();
        const newUrl = window.location.origin + window.location.pathname + (queryString === "" ? "" : `?${queryString}`);

        window.history.replaceState({ path: newUrl }, "", newUrl);

        setLocalFilterTagged(filter);
        setStoredFilterTagged(filter);
    }

    const [period, setPeriod] = useState<Period>({ startDate: null, endDate: null });
    const dispatch = useDispatch();

    const [open, setOpen] = useLocalStorage("filter-panel-open", false);


    const clear = () => {
        setFilterDescription("");
        setFilterTagged(false);
        setFilterTag(null);
    }

    useEffect(() => {
        dispatch(TransactionsSlice.actions.setTransactionListFilter({ description: filterDescription, filterTagged, tag: filterTag, start: period?.startDate?.toISOString(), end: period?.endDate?.toISOString() }));
    }, [period, filterDescription, filterTagged, filterTag, window.location.search]);

    return (
        <fieldset className="filter-panel box">
            <legend className={classnames("clickable", open && "open")} onClick={() => setOpen(!open)}><FontAwesomeIcon icon="chevron-down" size="xs" />Filters</legend>
            <div className="control-panel"><FontAwesomeIcon className="clickable" title="Clear filters" icon="filter-circle-xmark" onClick={clear} size="lg" aria-controls="filter-panel-collapse" /></div>
            <Collapse in={open}>
                <div id="filter-panel-collapse">
                    <Row>
                        <Col className="description">
                            <Form.Label htmlFor="filter-desc">Description</Form.Label><Tooltip id="filter-desc">Search for multiple terms by separating them with a comma</Tooltip>
                            <Form.Control id="filter-desc" type="search" value={filterDescription} onChange={(e) => setFilterDescription(e.currentTarget.value)} placeholder="Contains..." />
                        </Col>
                        <Col>
                            <Form.Label htmlFor="filter-desc">Tags</Form.Label>
                            {/*<TransactionTagPanel allowCreate={false} alwaysShowEditPanel value={filterTags} onChange={setFilterTags} />*/}
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
