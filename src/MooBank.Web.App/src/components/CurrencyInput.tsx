import { Form } from "@andrewmclachlan/moo-ds";
import type { InputProps } from "@andrewmclachlan/moo-ds";
import { InputGroup } from "@andrewmclachlan/moo-ds";
import { currencySymbols } from "utils/currency";

export interface CurrencyInputProps extends Omit<InputProps, "type"> {
    /** ISO currency code (e.g. "AUD", "USD"). Drives the input-group symbol; falls back to "$". */
    currency?: string;
}

export const CurrencyInput: React.FC<CurrencyInputProps> = ({ currency, ...props }) => (

    <InputGroup>
        <InputGroup.Text>{(currency && currencySymbols[currency.toUpperCase()]) || "$"}</InputGroup.Text>
        <Form.Input type="number" className="form-control" placeholder="0.00" maxLength={10} step={0.01} {...props} />
    </InputGroup>
)
