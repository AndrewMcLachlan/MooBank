import { Section, Tooltip, useLocalStorage } from "@andrewmclachlan/mooapp";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import React, { useEffect, useState } from "react";
import { Col, Form } from "react-bootstrap";
import { useDispatch, } from "react-redux";

import { PeriodSelector, FormRow as Row, TagSelector } from "components";
import { TransactionsSlice } from "store/Transactions";

import { Period } from "helpers/dateFns";

export const FilterPanel: React.FC<FilterPanelProps> = (props) => {

    const params = new URLSearchParams(window.location.search);

    const tagParams = params.get("tag")?.split(",").map(t => Number(t)) ?? [];

    const [storedFilterTagged, setStoredFilterTagged] = useLocalStorage("filter-tagged", false);
    const [filterDescription, setFilterDescription] = useLocalStorage("filter-description", "");
    const [storedFilterTags, setStoredFilterTags] = useLocalStorage<number[]>("filter-tag", []);

    const [localFilterTags, setLocalFilterTags] = useState<number[]>(tagParams ?? storedFilterTags);
    const [localFilterTagged, setLocalFilterTagged] = useState<boolean>(tagParams.length > 0 ? false : params.get("untagged") ? true : storedFilterTagged);
    const filterTags = localFilterTags ?? storedFilterTags;
    const filterTagged = localFilterTagged ?? storedFilterTagged;

    const setFilterTags = (tag: number | number[]) => {
        params.delete("tag");
        const queryString = params.toString();
        const newUrl = window.location.origin + window.location.pathname + (queryString === "" ? "" : `?${queryString}`);

        window.history.replaceState({ path: newUrl }, "", newUrl);

        const tagArray = Array.isArray(tag) ? tag : [tag];

        setLocalFilterTags(tagArray);
        setStoredFilterTags(tagArray);
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

    const clear = () => {
        setFilterDescription("");
        setFilterTagged(false);
        setFilterTags([]);
    }

    useEffect(() => {
        dispatch(TransactionsSlice.actions.setTransactionListFilter({ description: filterDescription, filterTagged, tags: filterTags, start: period?.startDate?.toISOString(), end: period?.endDate?.toISOString() }));
    }, [period, filterDescription, filterTagged, filterTags, window.location.search]);

    return (
        <Section className="filter-panel" title="Filter" {...props}>
            <div className="control-panel"><FontAwesomeIcon className="clickable" title="Clear filters" icon="filter-circle-xmark" onClick={clear} size="lg" aria-controls="filter-panel-collapse" /></div>
            <Row>
                <Col className="description">
                    <Form.Label htmlFor="filter-desc">Description</Form.Label><Tooltip id="filter-desc">Search for multiple terms by separating them with a comma</Tooltip>
                    <Form.Control id="filter-desc" type="search" value={filterDescription} onChange={(e) => setFilterDescription(e.currentTarget.value)} placeholder="Contains..." />
                </Col>
                <Col>
                    <Form.Label htmlFor="filter-desc">Tags</Form.Label>
                    <TagSelector onChange={setFilterTags} multiSelect value={filterTags} />
                </Col>
            </Row>
            <PeriodSelector instant onChange={setPeriod} />
            <Row>
                <Form.Group>
                    <Form.Check id="filter-tagged" label="Only show transactions without tags" checked={filterTagged} onChange={(e) => setFilterTagged(e.currentTarget.checked)} />
                </Form.Group>
            </Row>
        </Section>
    );
}

export interface FilterPanelProps extends React.HTMLAttributes<HTMLElement> {
}
