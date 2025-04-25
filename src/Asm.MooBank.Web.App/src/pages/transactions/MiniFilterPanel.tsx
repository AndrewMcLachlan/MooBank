import { ComboBox, Section, Tooltip, useLocalStorage } from "@andrewmclachlan/mooapp";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import React, { useEffect, useState } from "react";
import { Button, ButtonGroup, Col, Form } from "react-bootstrap";
import { useDispatch, } from "react-redux";

import { PeriodSelector, FormRow as Row, TagSelector } from "components";
import { TransactionsSlice } from "store/Transactions";

import { Period } from "helpers/dateFns";
import { transactionTypeFilter } from "store/state";
import { cleanQueryString } from "helpers/queryString";
import { MiniPeriodSelector } from "components/MiniPeriodSelector";

export const MiniFilterPanel: React.FC<MiniFilterPanelProps> = (props) => {

    const params = new URLSearchParams(window.location.search);

    const tagParams = params.get("tag")?.split(",").map(t => Number(t)) ?? [];
    const transactionTypeParam: transactionTypeFilter = params.get("type") as transactionTypeFilter ?? "";
    const unTaggedParam = params.get("untagged");

    const [storedFilterTagged, setStoredFilterTagged] = useLocalStorage("filter-tagged", false);
    const [filterDescription, setFilterDescription] = useLocalStorage("filter-description", "");
    const [storedFilterTags, setStoredFilterTags] = useLocalStorage<number[]>("filter-tag", []);
    const [storedFilterType, setStoredFilterType] = useLocalStorage<transactionTypeFilter>("filter-type", "");

    const [localFilterTags, setLocalFilterTags] = useState<number[]>(tagParams ?? storedFilterTags);
    const [localFilterTagged, setLocalFilterTagged] = useState<boolean>(unTaggedParam ? true : storedFilterTagged);
    const [localFilterType, setLocalFilterType] = useState<transactionTypeFilter>(transactionTypeParam ?? storedFilterType);
    const filterTags = localFilterTags ?? storedFilterTags;
    const filterTagged = localFilterTagged ?? storedFilterTagged;

    useEffect(() => {
        // If the URL has filters defined, clear the description filter.
        (transactionTypeParam || tagParams || unTaggedParam) && setFilterDescription("");
        tagParams && setStoredFilterTagged(false);
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
        <Section className="mini-filter-panel" {...props}>
            <Form.Control id="filter-desc" type="search" value={filterDescription} onChange={(e) => setFilterDescription(e.currentTarget.value)} placeholder="Description contains..." />
            <TagSelector id="filter-tags" onChange={setFilterTags} multiSelect value={filterTags} />
            <Form.Select aria-label="Filter by income or expense" id="filter-type">
                <option id="filter-all">All</option>
                <option id="filter-income">Income</option>
                <option id="filter-expense">Expense</option>
            </Form.Select>
            <MiniPeriodSelector instant onChange={setPeriod} />
            <Form.Switch id="filter-tagged" label="Untagged" checked={filterTagged} onChange={(e) => setFilterTagged(e.currentTarget.checked)} />
        </Section>
    );
}

export interface MiniFilterPanelProps extends React.HTMLAttributes<HTMLElement> {
}
