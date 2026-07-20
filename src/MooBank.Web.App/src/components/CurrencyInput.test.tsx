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

    // Amounts persist as decimal(12, 4). A coarser step makes the browser reject finer values
    // with a stepMismatch on submit, which previously capped every amount field at 2dp.
    it("accepts 4 decimal places", () => {
        renderCurrencyInput({ currency: "AUD" });
        const input = document.querySelector<HTMLInputElement>("#amount")!;

        expect(input).toHaveAttribute("step", "0.0001");

        input.value = "12.3456";
        expect(input.validity.stepMismatch).toBe(false);
    });

    it("lets a caller override the step", () => {
        renderCurrencyInput({ currency: "AUD", step: 1 });
        const input = document.querySelector<HTMLInputElement>("#amount")!;
        expect(input).toHaveAttribute("step", "1");
    });
});
