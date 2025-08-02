import { useLocalStorage } from "@andrewmclachlan/mooapp";
import { useEffect, useState } from "react";

import { Period } from "helpers/dateFns";
import { transactionTypeFilter } from "store/state";
import { cleanQueryString } from "helpers/queryString";

export const useFilterPanel = () => {

    const params = new URLSearchParams(window.location.search);

    const tagParams = params.get("tag")?.split(",").map(t => Number(t)) ?? [];
    const transactionTypeParam: transactionTypeFilter = params.get("type") as transactionTypeFilter ?? "";
    const unTaggedParam = params.get("untagged");
    const netZeroParam = params.get("netzero");

    const [storedFilterTagged, setStoredFilterTagged] = useLocalStorage("filter-tagged", false);
    const [storedFilterNetZero, setStoredFilterNetZero] = useLocalStorage("filter-netzero", false);
    const [filterDescription, setFilterDescription] = useLocalStorage("filter-description", "");
    const [storedFilterTags, setStoredFilterTags] = useLocalStorage<number[]>("filter-tag", []);
    const [storedFilterType, setStoredFilterType] = useLocalStorage<transactionTypeFilter>("filter-type", "");

    const [localFilterTags, setLocalFilterTags] = useState<number[]>(tagParams ?? storedFilterTags);
    const [localFilterTagged, setLocalFilterTagged] = useState<boolean>(unTaggedParam ? true : storedFilterTagged);
    const [localFilterNetZero, setLocalFilterNetZero] = useState<boolean>(netZeroParam ? true : storedFilterNetZero);
    const [localFilterType, setLocalFilterType] = useState<transactionTypeFilter>(transactionTypeParam ?? storedFilterType);
    const filterTags = localFilterTags ?? storedFilterTags;
    const filterTagged = localFilterTagged ?? storedFilterTagged;
    const filterNetZero = localFilterNetZero ?? storedFilterNetZero;

    useEffect(() => {
        // If the URL has filters defined, clear the description filter.
        if (transactionTypeParam || tagParams.length > 0 || unTaggedParam) setFilterDescription("");
        if (tagParams.length > 0) setStoredFilterTagged(false);
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

    const setFilterNetZero = (filter: boolean) => {
        cleanQueryString(params, "netzero");

        setLocalFilterNetZero(filter);
        setStoredFilterNetZero(filter);
    }

    const setFilterType = (type: transactionTypeFilter) => {
        cleanQueryString(params, "type");

        setStoredFilterType(type);
        setLocalFilterType(type);
    }


    const [period, setPeriod] = useState<Period>({ startDate: null, endDate: null });

    const clear = () => {
        setFilterDescription("");
        setFilterTagged(false);
        setFilterNetZero(false);
        setFilterTags([]);
        setStoredFilterType("");
    }

    return {
        filterDescription,
        filterTagged,
        filterNetZero,
        filterTags,
        filterType: localFilterType,
        storedFilterType,
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
