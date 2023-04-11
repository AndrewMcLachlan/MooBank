import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import React, { useRef } from "react";
import { Form, FormControlProps, FormControlElement } from "react-bootstrap";
import { BsPrefixRefForwardingComponent } from "react-bootstrap/esm/helpers";

export const Input: BsPrefixRefForwardingComponent<"input", InputProps> = ({clrearable, ref, ...props}) => {

    const tref = useRef<FormControlElement>();
    const theRef = ref ?? tref;

    const onClick = () => {
        var nativeInputValueSetter = Object.getOwnPropertyDescriptor(window.HTMLInputElement!.prototype, "value")!.set!;

        nativeInputValueSetter.call((theRef as any).current!, "");

        (theRef as any).current!.dispatchEvent(new Event("input", { bubbles: true }));

    }

    const formControlProps = {ref: theRef, ...props} as InputProps;

    return (
        <div className="clearable">
            <Form.Control {...formControlProps} />
            <FontAwesomeIcon icon="xmark" className="input-clear" onClick={onClick} size="lg" />
        </div>
    );
}

Input.defaultProps = {
    clearable: false,
}

export interface InputProps extends FormControlProps {
    clearable?: boolean;
}