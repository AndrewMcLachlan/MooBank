import { ValueProps } from "@andrewmclachlan/mooapp";
import classNames from "classnames";
import { isMonthSelected } from "helpers/dateFns";
import { ElementType, useState } from "react";

export const MonthSelector: React.FC<MonthSelectorProps<any>> = ({ className, as = "div", value: propsValue = 4095, ...props }) => {

    const [value, setValue] = useState(propsValue);

    const onChange: React.ChangeEventHandler<HTMLInputElement> = (e) => {
        const mask = 1 << Number(e.currentTarget.value);
        const newValue = value ^ mask;
        setValue(newValue);
        props.onChange?.(newValue);
    }

    return (
        <props.as className={classNames(className, "month-selector")}>
            <input type="checkbox" id="jan" value={0} checked={isMonthSelected(value, 0)} onChange={onChange} />
            <input type="checkbox" id="feb" value={1} checked={isMonthSelected(value, 1)} onChange={onChange} />
            <input type="checkbox" id="mar" value={2} checked={isMonthSelected(value, 2)} onChange={onChange} />
            <input type="checkbox" id="apr" value={3} checked={isMonthSelected(value, 3)} onChange={onChange} />
            <input type="checkbox" id="may" value={4} checked={isMonthSelected(value, 4)} onChange={onChange} />
            <input type="checkbox" id="jun" value={5} checked={isMonthSelected(value, 5)} onChange={onChange} />
            <input type="checkbox" id="jul" value={6} checked={isMonthSelected(value, 6)} onChange={onChange} />
            <input type="checkbox" id="aug" value={7} checked={isMonthSelected(value, 7)} onChange={onChange} />
            <input type="checkbox" id="sep" value={8} checked={isMonthSelected(value, 8)} onChange={onChange} />
            <input type="checkbox" id="oct" value={9} checked={isMonthSelected(value, 9)} onChange={onChange} />
            <input type="checkbox" id="nov" value={10} checked={isMonthSelected(value, 10)} onChange={onChange} />
            <input type="checkbox" id="dec" value={11} checked={isMonthSelected(value, 11)} onChange={onChange} />
            <svg height="20" width="20" viewBox="0 0 20 20" focusable="false" className="clickable clear" onClick={() => setValue(0)}><path d="M14.348 14.849c-0.469 0.469-1.229 0.469-1.697 0l-2.651-3.030-2.651 3.029c-0.469 0.469-1.229 0.469-1.697 0-0.469-0.469-0.469-1.229 0-1.697l2.758-3.15-2.759-3.152c-0.469-0.469-0.469-1.228 0-1.697s1.228-0.469 1.697 0l2.652 3.031 2.651-3.031c0.469-0.469 1.228-0.469 1.697 0s0.469 1.229 0 1.697l-2.758 3.152 2.758 3.15c0.469 0.469 0.469 1.229 0 1.698z"></path></svg>
        </props.as>
    );
}

export type MonthSelectorProps<TElement extends ElementType> = Props<TElement> & Omit<React.ComponentPropsWithoutRef<TElement>, keyof Props<TElement>>;

interface Props<TElement extends ElementType> extends ValueProps<number> {
    as?: TElement;
}
