import { Section } from "@andrewmclachlan/moo-ds";
import React from "react";
import { Form, Input } from "@andrewmclachlan/moo-ds";

import { TagSelector } from "components";

import { MiniPeriodSelector } from "components/MiniPeriodSelector";
import { useFilterPanel } from "../hooks/useFilterPanel";
import type { transactionTypeFilter } from "models/transactions";

export const MiniFilterPanel: React.FC<MiniFilterPanelProps> = (props) => {

    const { filterDescription, filterTagged, filterNetZero, filterTags, filterType, setFilterDescription, setFilterTagged, setFilterNetZero, setFilterTags, setFilterType, setPeriod } = useFilterPanel();

    return (
        <Section className="mini-filter-panel" {...props}>
            <Input id="filter-desc" type="search" value={filterDescription} onChange={(e) => setFilterDescription(e.currentTarget.value)} placeholder="Description contains..." />
            <TagSelector id="filter-tags" onChange={setFilterTags} multiSelect value={filterTags} />
            <Input.Select aria-label="Filter by income or expense" id="filter-type" value={filterType} onChange={(e) => setFilterType(e.currentTarget.value as transactionTypeFilter)}>
                <option id="filter-all">All</option>
                <option id="filter-income">Income</option>
                <option id="filter-expense">Expense</option>
            </Input.Select>
            <MiniPeriodSelector instant onChange={setPeriod} />
            <Input.Switch id="filter-tagged" label="Untagged" checked={filterTagged} onChange={(e) => setFilterTagged(e.currentTarget.checked)} />
                <Input.Switch id="filter-netzero" label="Exclude offset" checked={filterNetZero} onChange={(e) => setFilterNetZero(e.currentTarget.checked)} />
        </Section>
    );
}

export interface MiniFilterPanelProps extends React.HTMLAttributes<HTMLElement> {
}
