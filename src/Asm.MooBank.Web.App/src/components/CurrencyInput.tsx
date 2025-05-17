import { Form, InputProps } from "@andrewmclachlan/mooapp";
import { InputGroup } from "react-bootstrap";

export const CurrencyInput: React.FC<Omit<InputProps, "type">> = (props) => (

    <InputGroup>
        <InputGroup.Text>$</InputGroup.Text>
        <Form.Input type="number" className="form-control" placeholder="0.00" maxLength={10} step={0.01} {...props} />
    </InputGroup>
)
