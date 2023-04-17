import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { useRef } from "react";
import { Form, FormControlProps } from "react-bootstrap";
import { BsPrefixRefForwardingComponent } from "react-bootstrap/esm/helpers";

export const Input: BsPrefixRefForwardingComponent<"input", InputProps> = ({clearable, ref, ...rest}) => {

    const tref = useRef<any>();
    const theRef = ref ?? tref;

    const onClick = () => {
        var nativeInputValueSetter = Object.getOwnPropertyDescriptor(window.HTMLInputElement!.prototype, "value")!.set!;

        nativeInputValueSetter.call((theRef as any).current!, "");

        (theRef as any).current!.dispatchEvent(new Event("input", { bubbles: true }));

    }

    const formControlProps = {ref: theRef, ...rest} as InputProps;

    return (
        <div className={clearable && "clearable"}>
            <Form.Control {...formControlProps} />
            {clearable && <svg height="20" width="20" viewBox="0 0 20 20" aria-hidden="true" focusable="false" className="clear-input"><path d="M14.348 14.849c-0.469 0.469-1.229 0.469-1.697 0l-2.651-3.030-2.651 3.029c-0.469 0.469-1.229 0.469-1.697 0-0.469-0.469-0.469-1.229 0-1.697l2.758-3.15-2.759-3.152c-0.469-0.469-0.469-1.228 0-1.697s1.228-0.469 1.697 0l2.652 3.031 2.651-3.031c0.469-0.469 1.228-0.469 1.697 0s0.469 1.229 0 1.697l-2.758 3.152 2.758 3.15c0.469 0.469 0.469 1.229 0 1.698z"></path></svg>}
        </div>
    );
}

Input.defaultProps = {
    clearable: false,
}

export interface InputProps extends FormControlProps {
    clearable?: boolean;
}