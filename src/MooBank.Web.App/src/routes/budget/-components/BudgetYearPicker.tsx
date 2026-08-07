import { OverlayTrigger, Popover } from "@andrewmclachlan/moo-ds";

/**
 * Year navigation for the budget pages: arrows step to the adjacent year, and the year itself
 * opens the list of years you already have a budget for.
 *
 * Viewing a year creates its budget server-side (the `/budget/{year}` query creates on miss), so
 * the arrows stop one year either side of what exists — far enough to start the next or previous
 * year, not far enough for a held-down arrow to leave a trail of empty budgets.
 */
export const BudgetYearPicker: React.FC<BudgetYearPickerProps> = ({ year, years = [], onChange }) => {

    const existing = years.length > 0 ? [...years].sort((a, b) => a - b) : [year];
    const first = existing[0];
    const last = existing[existing.length - 1];

    const listed = [...new Set([...existing, year])].sort((a, b) => a - b);
    const nextNew = last + 1;

    return (
        <div className="budget-year-picker">
            <button type="button" className="year-step" onClick={() => onChange(year - 1)} disabled={year < first} aria-label={`Budget for ${year - 1}`}>‹</button>
            <OverlayTrigger trigger="click" placement="bottom" rootClose overlay={(close) => (
                <Popover id="budget-year-popover" className="budget-year-popover">
                    <Popover.Body>
                        <ul>
                            {listed.map(y =>
                                <li key={y}>
                                    <button type="button" className={y === year ? "current" : undefined} aria-current={y === year ? "true" : undefined} onClick={() => { onChange(y); close(); }}>{y}</button>
                                </li>
                            )}
                            {!listed.includes(nextNew) &&
                                <li>
                                    <button type="button" className="add" onClick={() => { onChange(nextNew); close(); }}>+ {nextNew}</button>
                                </li>
                            }
                        </ul>
                    </Popover.Body>
                </Popover>
            )}>
                <button type="button" className="year-current" aria-label={`Budget year ${year}. Choose a different year`}>{year}</button>
            </OverlayTrigger>
            <button type="button" className="year-step" onClick={() => onChange(year + 1)} disabled={year > last} aria-label={`Budget for ${year + 1}`}>›</button>
        </div>
    );
};

export interface BudgetYearPickerProps {
    year: number;
    /** Years that already have a budget, from `useBudgetYears()`. */
    years?: number[];
    onChange: (year: number) => void;
}
