import React, { PropsWithChildren, ReactNode } from "react";
import { useClickAway, ValueProps } from "@andrewmclachlan/mooapp";
import { ChangeEventHandler, FocusEventHandler, KeyboardEventHandler, useEffect, useRef, useState } from "react";

export const EditColumn2: React.FC<PropsWithChildren<EditColumnProps>> = ({ children, ...props }) => {

    const ref = useRef<HTMLInputElement>();

    const [editing, setEditing] = useState(false);

    useClickAway(setEditing, ref, () => {
        props.onChange && props.onChange(ref.current?.value ?? "");
    });

    const [value, setValue] = useState(props.value);

    const onChange: ChangeEventHandler<HTMLInputElement> = (e) => {
        setValue(e.currentTarget.value);
    }

    useEffect(() => {
        setValue(value);
    }, [props.value]);

    const onBlur: FocusEventHandler<HTMLInputElement> = (e) => {
        setEditing(false);
        props.onChange && props.onChange(e.currentTarget.value);
    }

    const onKey: KeyboardEventHandler<HTMLInputElement> = (e) => {
        if (e.key === "Enter" || e.key === "Tab") {
            setEditing(false);
            props.onChange && props.onChange(e.currentTarget.value);
        }
    }

    const child = <input className="form-control" type={props.type} value={value} onChange={onChange} onBlur={onBlur} onKeyUp={onKey} ref={ref} autoFocus />;

    return (
        <td onClick={() => setEditing(true)}>
            {!editing && (children ?? props.value)}
            {editing && child}
        </td>
    );
}

EditColumn2.defaultProps = {
    type: "text"
};

export interface EditColumnProps extends ValueProps<string> {
    type?: string;
}