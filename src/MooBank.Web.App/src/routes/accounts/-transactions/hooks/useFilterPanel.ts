import { useLocalStorage } from "@andrewmclachlan/moo-ds";
import { useEffect, useMemo, useState } from "react";
import { useLocation } from "@tanstack/react-router";

import type { Period } from "models/dateFns";
import type { transactionTypeFilter } from "store/state";
import { cleanQueryString } from "utils/queryString";

export const useFilterPanel = () => {

    // The router's search string is reactive; window.location.search is not.
    const searchStr = useLocation({ select: (location) => location.searchStr });

    const urlFilters = useMemo(() => {
        const params = new URLSearchParams(searchStr);
        const tagParam = params.get("tag");
        return {
            tags: tagParam ? tagParam.split(",").map(t => Number(t)) : null,
            type: params.get("type") as transactionTypeFilter | null,
            untagged: params.get("untagged") !== null,
            netZero: params.get("netzero") !== null,
        };
    }, [searchStr]);

    // Persisted filter values.
    const [storedFilterTagged, setStoredFilterTagged] = useLocalStorage("filter-tagged", false);
    const [storedFilterNetZero, setStoredFilterNetZero] = useLocalStorage("filter-netzero", false);
    const [filterDescription, setFilterDescription] = useLocalStorage("filter-description", "");
    const [storedFilterTags, setStoredFilterTags] = useLocalStorage<number[]>("filter-tag", []);
    const [storedFilterType, setStoredFilterType] = useLocalStorage<transactionTypeFilter>("filter-type", "");

    // Single source of truth for the applied filters: restored from the URL first, then localStorage.
    const [filterTags, setFilterTagsState] = useState<number[]>(urlFilters.tags ?? storedFilterTags);
    const [filterTagged, setFilterTaggedState] = useState<boolean>(urlFilters.untagged ? true : storedFilterTagged);
    const [filterNetZero, setFilterNetZeroState] = useState<boolean>(urlFilters.netZero ? true : storedFilterNetZero);
    const [filterType, setFilterTypeState] = useState<transactionTypeFilter>(urlFilters.type ?? storedFilterType);

    // Re-apply URL filters when the search string changes (e.g. navigating from a dashboard widget).
    useEffect(() => {
        if (urlFilters.tags) setFilterTagsState(urlFilters.tags);
        if (urlFilters.type !== null) setFilterTypeState(urlFilters.type);
        if (urlFilters.untagged) setFilterTaggedState(true);
        if (urlFilters.netZero) setFilterNetZeroState(true);

        // If the URL has filters defined, clear the description filter.
        if (urlFilters.type !== null || urlFilters.tags?.length || urlFilters.untagged) setFilterDescription("");
        if (urlFilters.tags?.length) {
            setFilterTaggedState(false);
            setStoredFilterTagged(false);
        }
    }, [urlFilters]);

    // Once a filter is changed by the user, its URL override no longer applies.
    const cleanParam = (key: string) => cleanQueryString(new URLSearchParams(window.location.search), key);

    const setFilterTags = (tag: number | number[]) => {
        cleanParam("tag");

        const tagArray = Array.isArray(tag) ? tag : [tag];

        setFilterTagsState(tagArray);
        setStoredFilterTags(tagArray);
    }

    const setFilterTagged = (filter: boolean) => {
        cleanParam("untagged");

        setFilterTaggedState(filter);
        setStoredFilterTagged(filter);
    }

    const setFilterNetZero = (filter: boolean) => {
        cleanParam("netzero");

        setFilterNetZeroState(filter);
        setStoredFilterNetZero(filter);
    }

    const setFilterType = (type: transactionTypeFilter) => {
        cleanParam("type");

        setFilterTypeState(type);
        setStoredFilterType(type);
    }

    const [period, setPeriod] = useState<Period>({ startDate: null, endDate: null });

    const clear = () => {
        setFilterDescription("");
        setFilterTagged(false);
        setFilterNetZero(false);
        setFilterTags([]);
        setFilterType("");
    }

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
