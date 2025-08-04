import { Section } from "@andrewmclachlan/mooapp";
import React, { useEffect } from "react";
import { Form } from "react-bootstrap";
import { useDispatch, } from "react-redux";

import { TagSelector } from "components";
import { TransactionsSlice } from "store/Transactions";

import { MiniPeriodSelector } from "components/MiniPeriodSelector";
import { useFilterPanel } from "../hooks/useFilterPanel";
import { transactionTypeFilter } from "store/state";

export const MiniFilterPanel: React.FC<MiniFilterPanelProps> = (props) => {

    const { filterDescription, filterTagged, filterNetZero, filterTags, filterType, storedFilterType, period, setFilterDescription, setFilterTagged, setFilterNetZero, setFilterTags, setFilterType, setPeriod } = useFilterPanel();
    const dispatch = useDispatch();


    useEffect(() => {
        dispatch(TransactionsSlice.actions.setTransactionListFilter({ description: filterDescription, filterTagged, tags: filterTags, transactionType: storedFilterType, start: period?.startDate?.toISOString(), end: period?.endDate?.toISOString() }));
    }, [period, filterDescription, filterTagged, filterTags, storedFilterType, window.location.search]);

    return (
        <Section className="mini-filter-panel" {...props}>
            <Form.Control id="filter-desc" type="search" value={filterDescription} onChange={(e) => setFilterDescription(e.currentTarget.value)} placeholder="Description contains..." />
            <TagSelector id="filter-tags" onChange={setFilterTags} multiSelect value={filterTags} />
            <Form.Select aria-label="Filter by income or expense" id="filter-type" value={filterType} onChange={(e) => setFilterType(e.currentTarget.value as transactionTypeFilter)}>
                <option id="filter-all">All</option>
                <option id="filter-income">Income</option>
                <option id="filter-expense">Expense</option>
            </Form.Select>
            <MiniPeriodSelector instant onChange={setPeriod} />
            <Form.Switch id="filter-tagged" label="Untagged" checked={filterTagged} onChange={(e) => setFilterTagged(e.currentTarget.checked)} />
                <Form.Switch id="filter-netzero" label="Exclude offset" checked={filterNetZero} onChange={(e) => setFilterNetZero(e.currentTarget.checked)} />
        </Section>
    );
}

export interface MiniFilterPanelProps extends React.HTMLAttributes<HTMLElement> {
}
