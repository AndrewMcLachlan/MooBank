import React, { useState } from "react";
import { Drawer, FilterBar, FilterChip, Input } from "@andrewmclachlan/moo-ds";

import { TagSelector } from "components";
import { DateRangeSelector } from "components/DateRangeSelector";
import { useTags } from "hooks/useTags";
import type { transactionTypeFilter } from "models/transactions";

import { useFilterPanel } from "../hooks/useFilterPanel";

/**
 * The filter on a phone: the period stays on the bar because it scopes the
 * figures in the header above it, and every other active filter shows as a chip
 * so the screen never lies about what it is showing.
 */
export const TransactionsFilterBar: React.FC = () => {

    const [showSheet, setShowSheet] = useState(false);
    const { data: tags } = useTags();

    const {
        filterDescription, filterTagged, filterNetZero, filterTags, filterType,
        clear, setFilterDescription, setFilterTagged, setFilterNetZero, setFilterTags, setFilterType, setPeriod,
    } = useFilterPanel();

    const tagName = (id: number) => tags?.find(t => t.id === id)?.name ?? String(id);

    const activeCount =
        (filterDescription ? 1 : 0) +
        filterTags.length +
        (filterType ? 1 : 0) +
        (filterTagged ? 1 : 0) +
        (filterNetZero ? 1 : 0);

    return (
        <>
            <FilterBar
                className="tx-filter-bar"
                primary={<DateRangeSelector onChange={setPeriod} />}
                activeCount={activeCount}
                onOpenFilters={() => setShowSheet(true)}
                onClear={activeCount > 0 ? clear : undefined}
            >
                {filterDescription && (
                    <FilterChip key="description" onRemove={() => setFilterDescription("")}>{filterDescription}</FilterChip>
                )}
                {filterTags.map(id => (
                    <FilterChip key={`tag-${id}`} onRemove={() => setFilterTags(filterTags.filter(t => t !== id))}>{tagName(id)}</FilterChip>
                ))}
                {filterType && (
                    <FilterChip key="type" onRemove={() => setFilterType("" as transactionTypeFilter)}>{filterType}</FilterChip>
                )}
                {filterTagged && (
                    <FilterChip key="untagged" onRemove={() => setFilterTagged(false)}>Untagged</FilterChip>
                )}
                {filterNetZero && (
                    <FilterChip key="offset" onRemove={() => setFilterNetZero(false)}>Excluding offset</FilterChip>
                )}
            </FilterBar>

            <Drawer show={showSheet} onHide={() => setShowSheet(false)} placement="bottom" className="tx-filter-sheet">
                <Drawer.Header closeButton><h2>Filters</h2></Drawer.Header>
                <Drawer.Body>
                    <Input id="filter-desc" type="search" value={filterDescription} onChange={(e) => setFilterDescription(e.currentTarget.value)} placeholder="Description contains..." />
                    <TagSelector id="filter-tags" onChange={setFilterTags} multiSelect value={filterTags} />
                    <Input.Select aria-label="Filter by income or expense" id="filter-type" value={filterType} onChange={(e) => setFilterType(e.currentTarget.value as transactionTypeFilter)}>
                        <option value="">All</option>
                        <option value="Income">Income</option>
                        <option value="Expense">Expense</option>
                    </Input.Select>
                    <Input.Switch id="filter-tagged" label="Only untagged" checked={filterTagged} onChange={(e) => setFilterTagged(e.currentTarget.checked)} />
                    <Input.Switch id="filter-netzero" label="Exclude fully offset" checked={filterNetZero} onChange={(e) => setFilterNetZero(e.currentTarget.checked)} />
                </Drawer.Body>
            </Drawer>
        </>
    );
};

TransactionsFilterBar.displayName = "TransactionsFilterBar";
