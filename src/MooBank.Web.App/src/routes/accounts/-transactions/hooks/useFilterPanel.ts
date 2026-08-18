import { useLocalStorage } from "@andrewmclachlan/moo-ds";
import { useEffect, useMemo, useState } from "react";

import type { Period } from "models/dateFns";
import { formatISODate } from "utils/dateFns";
import type { transactionTypeFilter } from "models/transactions";
import { useTransactionSearch } from "./useTransactionSearch";

export const useFilterPanel = () => {

    const { search, setFilter } = useTransactionSearch();

    // Seed the form once from the URL search params (shareable links, report/dashboard widgets);
    // where the URL is silent, fall back to the persisted defaults below. Read once on mount —
    // widgets navigate from other routes, so the transaction route always mounts fresh.
    const fromUrl = useMemo(() => ({
        tags: search.tags,
        type: search.type,
        tagged: search.tagged,
        netZero: search.netZero,
        description: search.description,
        hasWidgetFilter: !!(search.tags?.length || search.type || search.tagged),
    // eslint-disable-next-line react-hooks/exhaustive-deps
    }), []);

    // Persisted filter defaults.
    const [storedFilterTagged, setStoredFilterTagged] = useLocalStorage("filter-tagged", false);
    const [storedFilterNetZero, setStoredFilterNetZero] = useLocalStorage("filter-netzero", false);
    const [filterDescription, setFilterDescription] = useLocalStorage("filter-description", "");
    const [storedFilterTags, setStoredFilterTags] = useLocalStorage<number[]>("filter-tag", []);
    const [storedFilterType, setStoredFilterType] = useLocalStorage<transactionTypeFilter>("filter-type", "");

    // Applied filters: URL first, then localStorage.
    const [filterTags, setFilterTagsState] = useState<number[]>(fromUrl.tags ?? storedFilterTags);
    const [filterTagged, setFilterTaggedState] = useState<boolean>(fromUrl.tagged ?? (fromUrl.hasWidgetFilter ? false : storedFilterTagged));
    const [filterNetZero, setFilterNetZeroState] = useState<boolean>(fromUrl.netZero ?? storedFilterNetZero);
    const [filterType, setFilterTypeState] = useState<transactionTypeFilter>(fromUrl.type ?? storedFilterType);

    const [period, setPeriod] = useState<Period>({ startDate: null, endDate: null });

    // Arriving from a widget filter clears the description so it doesn't stack with the tag/type filter.
    useEffect(() => {
        if (fromUrl.hasWidgetFilter) setFilterDescription("");
        if (fromUrl.tags?.length) {
            setFilterTaggedState(false);
            setStoredFilterTagged(false);
        }
    // eslint-disable-next-line react-hooks/exhaustive-deps
    }, []);

    // Push the resolved filter to the route search params (formerly a Redux dispatch). setFilter
    // returns to page 1 whenever the filter changes. The query itself is debounced in
    // useTransactionSearch, so typing doesn't fire a request per keystroke.
    useEffect(() => {
        setFilter({
            description: filterDescription || undefined,
            tagged: filterTagged || undefined,
            netZero: filterNetZero || undefined,
            tags: filterTags?.length ? filterTags : undefined,
            type: filterType || undefined,
            start: period?.startDate && formatISODate(period.startDate),
            end: period?.endDate && formatISODate(period.endDate),
        });
    // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [period, filterDescription, filterTagged, filterNetZero, filterTags, filterType]);

    const setFilterTags = (tag: number | number[]) => {
        const tagArray = Array.isArray(tag) ? tag : [tag];
        setFilterTagsState(tagArray);
        setStoredFilterTags(tagArray);
    };

    const setFilterTagged = (value: boolean) => {
        setFilterTaggedState(value);
        setStoredFilterTagged(value);
    };

    const setFilterNetZero = (value: boolean) => {
        setFilterNetZeroState(value);
        setStoredFilterNetZero(value);
    };

    const setFilterType = (type: transactionTypeFilter) => {
        setFilterTypeState(type);
        setStoredFilterType(type);
    };

    const clear = () => {
        setFilterDescription("");
        setFilterTagged(false);
        setFilterNetZero(false);
        setFilterTags([]);
        setFilterType("");
    };

    return {
        filterDescription,
        filterTagged,
        filterNetZero,
        filterTags,
        filterType,
        period,
        clear,
        setFilterDescription,
        setFilterTagged,
        setFilterNetZero,
        setFilterTags,
        setFilterType,
        setPeriod,
    };
};
