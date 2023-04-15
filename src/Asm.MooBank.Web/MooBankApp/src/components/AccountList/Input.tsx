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
            {clearable && <FontAwesomeIcon icon="xmark" className="input-clear" onClick={onClick} size="lg" />}
        </div>
    );
}

Input.defaultProps = {
    clearable: false,
}

export interface InputProps extends FormControlProps {
    clearable?: boolean;
}