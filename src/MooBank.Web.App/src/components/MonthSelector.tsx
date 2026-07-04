import type { ValueProps } from "@andrewmclachlan/moo-ds";
import classNames from "classnames";
import { isMonthSelected } from "utils/dateFns";
import type { ElementType } from "react";

const months = [
    { id: "jan", name: "January" },
    { id: "feb", name: "February" },
    { id: "mar", name: "March" },
    { id: "apr", name: "April" },
    { id: "may", name: "May" },
    { id: "jun", name: "June" },
    { id: "jul", name: "July" },
    { id: "aug", name: "August" },
    { id: "sep", name: "September" },
    { id: "oct", name: "October" },
    { id: "nov", name: "November" },
    { id: "dec", name: "December" },
];

const allMonths = 4095;                 // 0b111111111111

export const MonthSelector: React.FC<MonthSelectorProps<any>> = ({ className, as = "div", value = 4095, ...props }) => {

    const onChange: React.ChangeEventHandler<HTMLInputElement> = (e) => {
        const mask = 1 << Number(e.currentTarget.value);
        const newValue = value ^ mask;
        props.onChange?.(newValue);
    }

    const As = as;

    return (
        <As className={classNames(className, "month-selector")} role="group" aria-label="Months the budget applies to">
            {months.map((m, i) =>
                <input key={m.id} type="checkbox" id={m.id} value={i} title={m.name} aria-label={m.name} checked={isMonthSelected(value, i)} onChange={onChange} />
            )}
            <span className="month-presets">
                <button type="button" className="month-preset" title="Every month" onClick={() => props.onChange?.(allMonths)}>All</button>
            </span>
            <svg height="20" width="20" viewBox="0 0 20 20" focusable="false" role="button" aria-label="Clear all months" className="clickable clear" onClick={() => props.onChange?.(0)}><path d="M14.348 14.849c-0.469 0.469-1.229 0.469-1.697 0l-2.651-3.030-2.651 3.029c-0.469 0.469-1.229 0.469-1.697 0-0.469-0.469-0.469-1.229 0-1.697l2.758-3.15-2.759-3.152c-0.469-0.469-0.469-1.228 0-1.697s1.228-0.469 1.697 0l2.652 3.031 2.651-3.031c0.469-0.469 1.228-0.469 1.697 0s0.469 1.229 0 1.697l-2.758 3.152 2.758 3.15c0.469 0.469 0.469 1.229 0 1.698z"></path></svg>
        </As>
    );
}

export type MonthSelectorProps<TElement extends ElementType> = Props<TElement> & Omit<React.ComponentPropsWithoutRef<TElement>, keyof Props<TElement>>;

interface Props<TElement extends ElementType> extends ValueProps<number> {
    as?: TElement;
}
