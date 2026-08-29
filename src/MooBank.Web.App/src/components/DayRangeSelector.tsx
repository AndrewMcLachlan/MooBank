import React, { useEffect, useMemo, useState } from "react";

import { OverlayTrigger, Popover } from "@andrewmclachlan/moo-ds";
import classNames from "classnames";
import { addMonths, eachDayOfInterval, endOfMonth, endOfWeek, format, isSameMonth, parseISO, startOfMonth, startOfWeek } from "date-fns";

import type { Period } from "models/dateFns";
import { periodOptions } from "models/periodOptions";
import { formatDateRange, formatISODate, formatPeriod } from "utils/dateFns";

/** The week starts on Monday, as it does on every Australian calendar. */
const weekStartsOn = 1 as const;

const orderPair = (a: string, b: string): [string, string] => a <= b ? [a, b] : [b, a];

const toPeriod = (start: string, end: string): Period => ({ startDate: parseISO(start), endDate: parseISO(end) });

/**
 * A range of days, chosen a day at a time.
 *
 * The sibling of <see cref="DateRangeSelector"/>, which works in whole months: same trigger of
 * fixed width, same popover, same two-click range. It differs in two ways that matter.
 *
 * It is controlled rather than self-managing. The month selector owns its own state and persists it
 * for the whole app, which is right for a filter every report shares; this one is handed a value,
 * because its consumers already own theirs -- the bills filter persists its own, and inside a form
 * the range is a field being edited.
 *
 * And it always holds a range. There is no empty state to clear to: resetting a filter leaves the
 * period alone, exactly as the transaction filters do, and a bill period always has two ends.
 */
export const DayRangeSelector: React.FC<DayRangeSelectorProps> = ({ value, onChange, presets = true, className, id = "day-range" }) => {

    const label = formatDateRange(formatISODate(value.startDate), formatISODate(value.endDate));

    return (
        <OverlayTrigger trigger="click" placement="bottom" rootClose overlay={(close) => (
            <Popover id={`${id}-popover`} className="date-range-popover">
                <Popover.Body>
                    <DayRangePanel value={value} onSelect={onChange} onClose={close} presets={presets} />
                </Popover.Body>
            </Popover>
        )}>
            <button type="button" id={id} className={classNames("form-select", "date-range-trigger", "day-range-trigger", className)} aria-haspopup="dialog" aria-label={`Dates: ${label}. Change the dates`}>{label}</button>
        </OverlayTrigger>
    );
};

export interface DayRangeSelectorProps {
    value: Period;
    onChange: (value: Period) => void;
    /** The ready-made periods. On by default; a form editing one bill's period has no use for them. */
    presets?: boolean;
    id?: string;
    className?: string;
}

/**
 * The popover contents. Exported for tests, which drive it directly: `OverlayTrigger` positions
 * itself with CSS anchor positioning in the top layer, neither of which jsdom implements.
 */
export const DayRangePanel: React.FC<DayRangePanelProps> = ({ value, onSelect, onClose, presets = true }) => {

    // Opens on the month the range ends in -- the end is what you are most likely to be adjusting
    // from, and a range that starts in the previous month would otherwise open a month early.
    const [month, setMonth] = useState(() => startOfMonth(value.endDate));

    // The first click of a range. Held here rather than committed, so a half-made range never
    // reaches the consumer and fires a query for dates you did not ask for.
    const [pendingStart, setPendingStart] = useState<string>(null);
    const [hoverDay, setHoverDay] = useState<string>(null);

    useEffect(() => {
        const onKeyDown = (e: KeyboardEvent) => e.key === "Escape" && onClose();
        document.addEventListener("keydown", onKeyDown);
        return () => document.removeEventListener("keydown", onKeyDown);
    }, [onClose]);

    // Whole weeks, so the grid is rectangular and the columns line up under their weekday headings.
    // Days from the neighbouring months are shown greyed rather than blank: a range often starts in
    // one month and ends in the next, and a hole in the grid makes that harder to see.
    const days = useMemo(() => eachDayOfInterval({
        start: startOfWeek(startOfMonth(month), { weekStartsOn }),
        end: endOfWeek(endOfMonth(month), { weekStartsOn }),
    }).map(date => ({
        key: formatISODate(date),
        day: format(date, "d"),
        long: format(date, "d MMMM yyyy"),
        outside: !isSameMonth(date, month),
    })), [month]);

    const weekdays = useMemo(() => eachDayOfInterval({
        start: startOfWeek(new Date(), { weekStartsOn }),
        end: endOfWeek(new Date(), { weekStartsOn }),
    }).map(d => ({ key: format(d, "i"), short: format(d, "EEEEE"), long: format(d, "EEEE") })), []);

    const current: [string, string] = [formatISODate(value.startDate), formatISODate(value.endDate)];

    // What the grid highlights: the range being built if there is one, otherwise the current value.
    const activeRange: [string, string] = pendingStart
        ? orderPair(pendingStart, hoverDay ?? pendingStart)
        : current;

    const resolved = pendingStart ? toPeriod(activeRange[0], activeRange[1]) : value;

    const selectDay = (day: string) => {
        if (!pendingStart) {
            setPendingStart(day);
            return;
        }

        const [start, end] = orderPair(pendingStart, day);
        setPendingStart(null);
        setHoverDay(null);
        onSelect(toPeriod(start, end));
        onClose();
    };

    return (
        <div className="date-range-panel day-range-panel">
            <div className="date-range-custom">
                <div className="date-range-year">
                    <button type="button" className="year-step" aria-label={`Show ${format(addMonths(month, -1), "MMMM yyyy")}`} onClick={() => setMonth(addMonths(month, -1))}>‹</button>
                    <span className="year-current">{format(month, "MMMM yyyy")}</span>
                    <button type="button" className="year-step" aria-label={`Show ${format(addMonths(month, 1), "MMMM yyyy")}`} onClick={() => setMonth(addMonths(month, 1))}>›</button>
                </div>
                <div className="day-range-weekdays" aria-hidden="true">
                    {weekdays.map(d => <span key={d.key} title={d.long}>{d.short}</span>)}
                </div>
                <div className="day-range-days" role="group" aria-label={pendingStart ? "Choose the day the range ends on" : "Choose the day the range starts on"} onMouseLeave={() => setHoverDay(null)}>
                    {days.map(d => {
                        const inRange = d.key >= activeRange[0] && d.key <= activeRange[1];
                        return (
                            <button
                                key={d.key}
                                type="button"
                                aria-label={d.long}
                                aria-pressed={inRange}
                                className={classNames(
                                    d.outside ? "outside" : undefined,
                                    inRange ? "in-range" : undefined,
                                    activeRange[0] === d.key ? "range-start" : undefined,
                                    activeRange[1] === d.key ? "range-end" : undefined,
                                )}
                                onClick={() => selectDay(d.key)}
                                onMouseEnter={() => setHoverDay(d.key)}
                            >{d.day}</button>
                        );
                    })}
                </div>
            </div>
            {presets && (
                <ul className="date-range-presets" aria-label="Ready-made periods">
                    {periodOptions.map(o => {
                        // Matched on the dates themselves: this control holds a range, not which
                        // preset produced it, so a preset reads as current when it resolves to what
                        // is selected -- however that selection was arrived at.
                        const isCurrent = formatISODate(o.startDate) === current[0] && formatISODate(o.endDate) === current[1];
                        return (
                            <li key={o.value}>
                                <button type="button" className={isCurrent ? "current" : undefined} aria-current={isCurrent ? "true" : undefined} onClick={() => { onSelect({ startDate: o.startDate, endDate: o.endDate }); onClose(); }}>{o.label}</button>
                            </li>
                        );
                    })}
                </ul>
            )}
            <p className="date-range-resolved" aria-live="polite">{formatPeriod(resolved)}</p>
        </div>
    );
};

export interface DayRangePanelProps {
    value: Period;
    onSelect: (value: Period) => void;
    onClose: () => void;
    presets?: boolean;
}
