import { useCallback, useMemo, useState } from "react";

import type { Period } from "models/dateFns";
import { periodOptions } from "models/periodOptions";
import { formatMonthRange, isMonth, monthRange } from "utils/dateFns";

/**
 * The selection behind the date range selector: either one of the ready-made periods, or a custom
 * range expressed in whole months. Both live in one stored value, so there is no way to be "on a
 * preset" and "on a custom range" at the same time.
 */
export type DateRangeSelection =
    | { preset: string }
    | { startMonth: string; endMonth: string };

export const dateRangeStorageKey = "date-range";

/** Last month — the period every consumer opened on before this control existed. */
const defaultPreset = "1";

export const isPresetSelection = (selection: DateRangeSelection): selection is { preset: string } =>
    "preset" in selection;

const findPreset = (value: string) => periodOptions.find(o => o.value === value);

const isValidSelection = (selection: DateRangeSelection) =>
    isPresetSelection(selection)
        ? !!findPreset(selection.preset)
        : isMonth(selection.startMonth) && isMonth(selection.endMonth);

/**
 * Resolves a selection to actual dates. Presets return the `periodOptions` entry itself: its
 * start/end are getters, so a long-lived tab keeps serving a live "Last month" rather than the
 * month that was current when the tab was opened.
 */
export const resolveDateRange = (selection: DateRangeSelection): Period =>
    isPresetSelection(selection)
        ? findPreset(selection.preset) ?? findPreset(defaultPreset)!
        : monthRange(selection.startMonth, selection.endMonth);

/** Text for the trigger button: the preset's own name, or a compact month range. */
export const dateRangeLabel = (selection: DateRangeSelection): string =>
    isPresetSelection(selection)
        ? findPreset(selection.preset)?.label ?? findPreset(defaultPreset)!.label
        : formatMonthRange(selection.startMonth, selection.endMonth);

/**
 * Reads the stored selection. A `?period=X` query parameter wins so that a link built elsewhere
 * (the dashboard widgets link to reports with `?period=1`) can scope the page it opens; it seeds
 * the selection without being written back, so it never overwrites the user's own choice.
 */
export const readDateRangeSelection = (): DateRangeSelection => {
    const urlPreset = new URLSearchParams(window.location.search).get("period");
    if (urlPreset && findPreset(urlPreset)) return { preset: urlPreset };

    const stored = localStorage.getItem(dateRangeStorageKey);
    if (stored) {
        try {
            const selection = JSON.parse(stored) as DateRangeSelection;
            if (selection && isValidSelection(selection)) return selection;
        } catch {
            // Unparseable storage falls through to the default below.
        }
    }

    return { preset: defaultPreset };
};

export const writeDateRangeSelection = (selection: DateRangeSelection) =>
    localStorage.setItem(dateRangeStorageKey, JSON.stringify(selection));

/**
 * The stored range, resolved to dates. For components that need a starting value before the
 * selector has mounted — `useState` seeds and the report route loaders.
 */
export const getDateRange = (): Period => resolveDateRange(readDateRangeSelection());

export const useDateRange = () => {
    const [selection, setSelection] = useState<DateRangeSelection>(readDateRangeSelection);

    const select = useCallback((value: DateRangeSelection) => {
        setSelection(value);
        writeDateRangeSelection(value);
    }, []);

    const period = useMemo(() => resolveDateRange(selection), [selection]);

    return { selection, setSelection: select, period };
};
