import { describe, it, expect } from "vitest";
import { render, screen } from "@testing-library/react";
import { FormProvider, useForm } from "react-hook-form";
import { CurrencyInput } from "components/CurrencyInput";

// CurrencyInput renders a moo-ds Form.Input, which reads `register` off react-hook-form's
// FormContext. Provide a minimal real form context rather than mocking react-hook-form.
const FormWrapper: React.FC<{ children: React.ReactNode }> = ({ children }) => {
    const form = useForm();
    return <FormProvider {...form}>{children}</FormProvider>;
};

// react-hook-form's register() needs a real field name to wire its ref, so every render passes
// an id (as CurrencyInput's real callers always do via Form.Group's groupId).
const renderCurrencyInput = (props: Partial<React.ComponentProps<typeof CurrencyInput>> = {}) =>
    render(<CurrencyInput id="amount" {...props} />, { wrapper: FormWrapper });

describe("CurrencyInput", () => {
    it("shows the symbol for a known currency", () => {
        renderCurrencyInput({ currency: "GBP" });
        expect(screen.getByText("£")).toBeInTheDocument();
    });

    it("falls back to $ when no currency is given", () => {
        renderCurrencyInput();
        expect(screen.getByText("$")).toBeInTheDocument();
    });

    it("falls back to $ when currency is an empty string", () => {
        renderCurrencyInput({ currency: "" });
        expect(screen.getByText("$")).toBeInTheDocument();
    });

    it("falls back to $ for an unrecognised currency code", () => {
        renderCurrencyInput({ currency: "XYZ" });
        expect(screen.getByText("$")).toBeInTheDocument();
    });

    it("renders a number input", () => {
        renderCurrencyInput({ currency: "AUD" });
        const input = document.querySelector<HTMLInputElement>("#amount")!;
        expect(input).toHaveAttribute("type", "number");
    });
});
