import { Section, Tooltip, useLocalStorage } from "@andrewmclachlan/mooapp";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import React, { useEffect, useState } from "react";
import { Button, ButtonGroup, Col, Form } from "react-bootstrap";
import { useDispatch, } from "react-redux";

import { PeriodSelector, FormRow as Row, TagSelector } from "components";
import { TransactionsSlice } from "store/Transactions";

import { Period } from "helpers/dateFns";
import { transactionTypeFilter } from "store/state";

export const FilterPanel: React.FC<FilterPanelProps> = (props) => {

    const params = new URLSearchParams(window.location.search);

    const tagParams = params.get("tag")?.split(",").map(t => Number(t)) ?? [];
    const transactionTypeParam: transactionTypeFilter = params.get("type") as transactionTypeFilter ?? "";
    const unTaggedParam = params.get("untagged");

    const [storedFilterTagged, setStoredFilterTagged] = useLocalStorage("filter-tagged", false);
    const [filterDescription, setFilterDescription] = useLocalStorage("filter-description", "");
    const [storedFilterTags, setStoredFilterTags] = useLocalStorage<number[]>("filter-tag", []);
    const [storedFilterType, setStoredFilterType] = useLocalStorage<transactionTypeFilter>("filter-type", "");

    const [localFilterTags, setLocalFilterTags] = useState<number[]>(tagParams ?? storedFilterTags);
    const [localFilterTagged, setLocalFilterTagged] = useState<boolean>(tagParams.length > 0 ? false : unTaggedParam ? true : storedFilterTagged);
    const [localFilterType, setLocalFilterType] = useState<transactionTypeFilter>(transactionTypeParam ?? storedFilterType);
    const filterTags = localFilterTags ?? storedFilterTags;
    const filterTagged = localFilterTagged ?? storedFilterTagged;

    useEffect(() => {
        // If the URL has filters defined, clear the description filter.
        transactionTypeParam || tagParams || unTaggedParam && setFilterDescription("");
    }, []);

    const setFilterTags = (tag: number | number[]) => {
        cleanQueryString(params, "tag");

        const tagArray = Array.isArray(tag) ? tag : [tag];

        setLocalFilterTags(tagArray);
        setStoredFilterTags(tagArray);
    }

    const setFilterTagged = (filter: boolean) => {
        cleanQueryString(params, "untagged");
        
        setLocalFilterTagged(filter);
        setStoredFilterTagged(filter);
    }

    const setFilterType = (type: transactionTypeFilter) => {
        cleanQueryString(params, "type");
        
        setStoredFilterType(type);
        setLocalFilterType(type);
    }


    const [period, setPeriod] = useState<Period>({ startDate: null, endDate: null });
    const dispatch = useDispatch();

    const clear = () => {
        setFilterDescription("");
        setFilterTagged(false);
        setFilterTags([]);
        setStoredFilterType("");
    }

    useEffect(() => {
        dispatch(TransactionsSlice.actions.setTransactionListFilter({ description: filterDescription, filterTagged, tags: filterTags, transactionType: storedFilterType, start: period?.startDate?.toISOString(), end: period?.endDate?.toISOString() }));
    }, [period, filterDescription, filterTagged, filterTags, storedFilterType, window.location.search]);

    return (
        <Section className="filter-panel" title="Filter" {...props}>
            <div className="control-panel"><FontAwesomeIcon className="clickable" title="Clear filters" icon="filter-circle-xmark" onClick={clear} size="lg" aria-controls="filter-panel-collapse" /></div>
            <Row>
                <Col className="description" lg={5}>
                    <Form.Label htmlFor="filter-desc">Description</Form.Label><Tooltip id="filter-desc">Search for multiple terms by separating them with a comma</Tooltip>
                    <Form.Control id="filter-desc" type="search" value={filterDescription} onChange={(e) => setFilterDescription(e.currentTarget.value)} placeholder="Contains..." />
                </Col>
                <Col lg={4}>
                    <Form.Label htmlFor="filter-tags">Tags</Form.Label>
                    <TagSelector id="filter-tags" onChange={setFilterTags} multiSelect value={filterTags} />
                </Col>
                <Col lg={3}>
                    <Form.Label htmlFor="filter-type">Type</Form.Label>
                    <ButtonGroup className="btn-group-form" aria-label="Filter by income or expense" id="filter-type">
                        <Button id="filter-all" variant={localFilterType === "" ? "primary" : "outline-primary"} onClick={() => setFilterType("")}>All</Button>
                        <Button id="filter-income" variant={localFilterType === "Credit" ? "primary" : "outline-primary"} onClick={() => setFilterType("Credit")}>Income</Button>
                        <Button id="filter-expense" variant={localFilterType === "Debit" ? "primary" : "outline-primary"} onClick={() => setFilterType("Debit")}>Expense</Button>
                    </ButtonGroup>
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

const cleanQueryString = (params: URLSearchParams, key: string) =>{
    
    params.delete(key);
    const queryString = params.toString();
    const newUrl = window.location.origin + window.location.pathname + (queryString === "" ? "" : `?${queryString}`);

    window.history.replaceState({ path: newUrl }, "", newUrl);
}