import { CloseBadge } from "@andrewmclachlan/mooapp";
import classNames from "classnames";
import { ElementType, PropsWithChildren, useState } from "react";

export const MonthSelector: React.FC<MonthSelectorProps<any>> = ({ className, ...props }) => {

    const [value, setValue] = useState(4095);

    const onChange: React.ChangeEventHandler<HTMLInputElement> = (e) => {
        var mask = 1 << Number(e.currentTarget.value);
        setValue(value ^ mask);
    }

    const isChecked = (month: number) => (value & (1 << month)) !== 0;

    return (
        

        <props.as className={classNames(className, "month-selector")}>
            <input type="checkbox" id="jan" value={0} checked={isChecked(0)} onChange={onChange} />
            <input type="checkbox" id="feb" value={1} checked={isChecked(1)} onChange={onChange} />
            <input type="checkbox" id="mar" value={2} checked={isChecked(2)} onChange={onChange} />
            <input type="checkbox" id="apr" value={3} checked={isChecked(3)} onChange={onChange} />
            <input type="checkbox" id="may" value={4} checked={isChecked(4)} onChange={onChange} />
            <input type="checkbox" id="jun" value={5} checked={isChecked(5)} onChange={onChange} />
            <input type="checkbox" id="jul" value={6} checked={isChecked(6)} onChange={onChange} />
            <input type="checkbox" id="aug" value={7} checked={isChecked(7)} onChange={onChange} />
            <input type="checkbox" id="sep" value={8} checked={isChecked(8)} onChange={onChange} />
            <input type="checkbox" id="oct" value={9} checked={isChecked(9)} onChange={onChange} />
            <input type="checkbox" id="nov" value={10} checked={isChecked(10)} onChange={onChange} />
            <input type="checkbox" id="dec" value={11} checked={isChecked(11)} onChange={onChange} />
            <svg height="20" width="20" viewBox="0 0 20 20" focusable="false" className="clickable clear" onClick={() => setValue(0)}><path d="M14.348 14.849c-0.469 0.469-1.229 0.469-1.697 0l-2.651-3.030-2.651 3.029c-0.469 0.469-1.229 0.469-1.697 0-0.469-0.469-0.469-1.229 0-1.697l2.758-3.15-2.759-3.152c-0.469-0.469-0.469-1.228 0-1.697s1.228-0.469 1.697 0l2.652 3.031 2.651-3.031c0.469-0.469 1.228-0.469 1.697 0s0.469 1.229 0 1.697l-2.758 3.152 2.758 3.15c0.469 0.469 0.469 1.229 0 1.698z"></path></svg>
        </props.as>
    );
}

MonthSelector.defaultProps = {
    as: "div"
}

export type MonthSelectorProps<TElement extends ElementType> = Props<TElement> & Omit<React.ComponentPropsWithoutRef<TElement>, keyof Props<TElement>>;

interface Props<TElement extends ElementType> {
    as?: TElement;
}

