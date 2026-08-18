import React, { useEffect, useMemo, useState } from "react";

import { OverlayTrigger, Popover } from "@andrewmclachlan/moo-ds";
import classNames from "classnames";
import { format } from "date-fns/format";

import type { DateRangeSelection } from "hooks/dateRange";
import { dateRangeLabel, isPresetSelection, resolveDateRange, useDateRange } from "hooks/dateRange";
import type { Period } from "models/dateFns";
import { periodOptions } from "models/periodOptions";
import { formatMonth, formatPeriod, monthRange } from "utils/dateFns";

/**
 * Period filtering for reports and transaction lists: ready-made periods and a custom month range
 * in one popover, behind a trigger of fixed width. Every consumer gets the same control at the same
 * size, so a filter bar never has to reserve space for start and end date fields that are usually
 * hidden.
 */
export const DateRangeSelector: React.FC<DateRangeSelectorProps> = ({ onChange, className, id = "date-range" }) => {

    const { selection, setSelection, period } = useDateRange();

    // Consumers hold the resolved period in their own state; tell them about it on mount and on
    // every change. Presets resolve to the shared `periodOptions` entry, so a repeat of the same
    // preset hands back the same object and React bails out of the re-render.
    useEffect(() => {
        onChange?.(period);
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [selection]);

    const label = dateRangeLabel(selection);

    return (
        <OverlayTrigger trigger="click" placement="bottom" rootClose overlay={(close) => (
            <Popover id={`${id}-popover`} className="date-range-popover">
                <Popover.Body>
                    <DateRangePanel selection={selection} onSelect={setSelection} onClose={close} />
                </Popover.Body>
            </Popover>
        )}>
            <button type="button" id={id} className={classNames("date-range-trigger", className)} aria-haspopup="dialog" aria-label={`Period: ${label}. Change the period`}>
                <span className="date-range-value">{label}</span>
                <span className="date-range-caret" aria-hidden="true">▾</span>
            </button>
        </OverlayTrigger>
    );
};

export interface DateRangeSelectorProps {
    onChange?: (value: Period) => void;
    id?: string;
    className?: string;
}

const orderPair = (a: string, b: string): [string, string] => a <= b ? [a, b] : [b, a];

/**
 * The popover contents. Exported for tests, which drive it directly: `OverlayTrigger` positions
 * itself with CSS anchor positioning in the top layer, neither of which jsdom implements.
 */
export const DateRangePanel: React.FC<DateRangePanelProps> = ({ selection, onSelect, onClose }) => {

    // Opens on the year the current range ends in — the end is what you are most likely to be
    // adjusting from, and in January "Last month" is in last year.
    const [year, setYear] = useState(() => resolveDateRange(selection).endDate.getFullYear());

    // The first click of a custom range. Held here rather than committed, so a half-made range
    // never reaches the consumer and fires a query for months you did not ask for.
    const [pendingStart, setPendingStart] = useState<string>(null);
    const [hoverMonth, setHoverMonth] = useState<string>(null);

    useEffect(() => {
        const onKeyDown = (e: KeyboardEvent) => e.key === "Escape" && onClose();
        document.addEventListener("keydown", onKeyDown);
        return () => document.removeEventListener("keydown", onKeyDown);
    }, [onClose]);

    const months = useMemo(() => Array.from({ length: 12 }, (_, month) => {
        const date = new Date(year, month, 1);
        return { key: formatMonth(date), short: format(date, "MMM"), long: format(date, "MMMM yyyy") };
    }), [year]);

    const selected = resolveDateRange(selection);

    // What the grid highlights: the range being built if there is one, otherwise the months the
    // current selection covers. Presets highlight too — picking "Last 3 months" and seeing which
    // three they are is the point of having both halves in one popover.
    const activeRange: [string, string] = pendingStart
        ? orderPair(pendingStart, hoverMonth ?? pendingStart)
        : [formatMonth(selected.startDate), formatMonth(selected.endDate)];

    const resolved = pendingStart ? monthRange(activeRange[0], activeRange[1]) : selected;

    const selectPreset = (preset: string) => {
        onSelect({ preset });
        onClose();
    };

    const selectMonth = (month: string) => {
        if (!pendingStart) {
            setPendingStart(month);
            return;
        }

        const [startMonth, endMonth] = orderPair(pendingStart, month);
        setPendingStart(null);
        setHoverMonth(null);
        onSelect({ startMonth, endMonth });
        onClose();
    };

    return (
        <div className="date-range-panel">
            <div className="date-range-custom">
                <div className="date-range-year">
                    <button type="button" className="year-step" aria-label={`Show ${year - 1}`} onClick={() => setYear(year - 1)}>‹</button>
                    <span className="year-current">{year}</span>
                    <button type="button" className="year-step" aria-label={`Show ${year + 1}`} onClick={() => setYear(year + 1)}>›</button>
                </div>
                <div className="date-range-months" role="group" aria-label={pendingStart ? "Choose the month the range ends in" : "Choose the month the range starts in"} onMouseLeave={() => setHoverMonth(null)}>
                    {months.map(m => {
                        const inRange = !!activeRange && m.key >= activeRange[0] && m.key <= activeRange[1];
                        return (
                            <button
                                key={m.key}
                                type="button"
                                aria-label={m.long}
                                aria-pressed={inRange}
                                className={classNames(
                                    inRange ? "in-range" : undefined,
                                    activeRange?.[0] === m.key ? "range-start" : undefined,
                                    activeRange?.[1] === m.key ? "range-end" : undefined,
                                )}
                                onClick={() => selectMonth(m.key)}
                                onMouseEnter={() => setHoverMonth(m.key)}
                            >{m.short}</button>
                        );
                    })}
                </div>
            </div>
            <ul className="date-range-presets" aria-label="Ready-made periods">
                {periodOptions.map(o => {
                    const current = isPresetSelection(selection) && selection.preset === o.value;
                    return (
                        <li key={o.value}>
                            <button type="button" className={current ? "current" : undefined} aria-current={current ? "true" : undefined} onClick={() => selectPreset(o.value)}>{o.label}</button>
                        </li>
                    );
                })}
            </ul>
            <p className="date-range-resolved" aria-live="polite">{formatPeriod(resolved)}</p>
        </div>
    );
};

export interface DateRangePanelProps {
    selection: DateRangeSelection;
    onSelect: (selection: DateRangeSelection) => void;
    onClose: () => void;
}
