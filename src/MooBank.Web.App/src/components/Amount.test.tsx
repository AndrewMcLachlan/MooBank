import { describe, it, expect } from "vitest";
import { render } from "@testing-library/react";
import { Amount } from "components/Amount";

const renderAmount = (props: React.ComponentProps<typeof Amount>) => {
    const { container } = render(<Amount {...props} />);
    return container.querySelector("span.amount")!;
};

describe("Amount", () => {
    it("renders a positive amount with two decimal places by default", () => {
        const el = renderAmount({ amount: 42 });
        expect(el).toHaveTextContent("42.00");
        expect(el).toHaveClass("amount");
        expect(el).not.toHaveClass("negative");
        expect(el).not.toHaveClass("positive");
    });

    it("renders a negative amount as its absolute value without a sign by default", () => {
        const el = renderAmount({ amount: -42 });
        expect(el).toHaveTextContent("42.00");
        expect(el).not.toHaveTextContent("-42.00");
    });

    it("applies the negative class when negativeColour is set and the amount is negative", () => {
        const el = renderAmount({ amount: -10, negativeColour: true });
        expect(el).toHaveClass("negative");
    });

    it("applies the positive class when positiveColour is set and the amount is positive", () => {
        const el = renderAmount({ amount: 10, positiveColour: true });
        expect(el).toHaveClass("positive");
    });

    it("does not colour a positive amount when only negativeColour is set", () => {
        const el = renderAmount({ amount: 10, negativeColour: true });
        expect(el).not.toHaveClass("negative");
        expect(el).not.toHaveClass("positive");
    });

    it("prefixes a minus sign for negative amounts when minus is set", () => {
        const el = renderAmount({ amount: -5, minus: true });
        expect(el).toHaveTextContent("-5.00");
    });

    it("prefixes a plus sign for positive amounts when plus is set", () => {
        const el = renderAmount({ amount: 5, plus: true });
        expect(el).toHaveTextContent("+5.00");
    });

    it("does not add a plus sign to a negative amount", () => {
        const el = renderAmount({ amount: -5, plus: true });
        expect(el.textContent).not.toContain("+");
    });

    it("appends a DR suffix for negative amounts when creditdebit is set", () => {
        const el = renderAmount({ amount: -5, creditdebit: true });
        expect(el).toHaveTextContent("5.00DR");
    });

    it("appends a CR suffix for positive amounts when creditdebit is set", () => {
        const el = renderAmount({ amount: 5, creditdebit: true });
        expect(el).toHaveTextContent("5.00CR");
    });

    it("uses the currency symbol for the given currency code", () => {
        const el = renderAmount({ amount: 10, currencyCode: "GBP" });
        expect(el).toHaveTextContent("£10.00");
    });

    it("shows no currency symbol when no currency code is given", () => {
        const el = renderAmount({ amount: 10 });
        expect(el.textContent).toBe("10.00");
    });

    it("treats a zero amount as neutral by default (no colour class)", () => {
        const el = renderAmount({ amount: 0, positiveColour: true, negativeColour: true });
        expect(el).not.toHaveClass("positive");
        expect(el).not.toHaveClass("negative");
    });

    it("colours a zero amount as negative when zeroShowsAs is negative", () => {
        const el = renderAmount({ amount: 0, zeroShowsAs: "negative" });
        expect(el).toHaveClass("negative");
    });

    it("colours a zero amount as positive when zeroShowsAs is positive", () => {
        const el = renderAmount({ amount: 0, zeroShowsAs: "positive" });
        expect(el).toHaveClass("positive");
    });

    it("treats a null amount as zero", () => {
        const el = renderAmount({ amount: null });
        expect(el).toHaveTextContent("0.00");
    });

    it("treats a NaN amount as zero", () => {
        const el = renderAmount({ amount: NaN });
        expect(el).toHaveTextContent("0.00");
    });

    it("respects a custom decimalPlaces value", () => {
        const el = renderAmount({ amount: 42, decimalPlaces: 0 });
        expect(el).toHaveTextContent("42");
        expect(el.textContent).not.toContain(".");
    });

    it("applies prefix and suffix text", () => {
        const el = renderAmount({ amount: 5, prefix: "~", suffix: " approx" });
        expect(el).toHaveTextContent("~5.00 approx");
    });
});
