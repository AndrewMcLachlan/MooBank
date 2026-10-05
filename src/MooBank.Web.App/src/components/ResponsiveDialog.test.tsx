import { describe, it, expect, vi } from "vitest";
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";

import { setViewportWidth } from "test/matchMedia";
import { ResponsiveDialog } from "./ResponsiveDialog";
import type { PhonePresentation } from "./ResponsiveDialog";

const phoneWidth = 390;

const renderDialog = ({ phone, onHide = vi.fn(), onSubmit = vi.fn() }: { phone?: PhonePresentation, onHide?: () => void, onSubmit?: () => void } = {}) => {
    const user = userEvent.setup();
    render(
        <ResponsiveDialog show onHide={onHide} size="lg" className="thing-dialog" phone={phone}>
            <form onSubmit={(e) => { e.preventDefault(); onSubmit(); }}>
                <ResponsiveDialog.Header closeButton>
                    <ResponsiveDialog.Title>Thing</ResponsiveDialog.Title>
                </ResponsiveDialog.Header>
                <ResponsiveDialog.Body>
                    <input aria-label="Name" />
                </ResponsiveDialog.Body>
                <ResponsiveDialog.Footer>
                    <button type="submit">Save</button>
                </ResponsiveDialog.Footer>
            </form>
        </ResponsiveDialog>
    );
    const container = screen.getByRole("dialog").parentElement;
    return { user, onHide, onSubmit, container };
};

describe("ResponsiveDialog", () => {

    it("is a centred modal on a desktop", () => {
        const { container } = renderDialog();

        expect(container).toHaveClass("modal", "modal-lg", "thing-dialog");
        expect(container).not.toHaveClass("sheet");
    });

    it("is a bottom sheet on a phone", () => {
        setViewportWidth(phoneWidth);

        const { container } = renderDialog();

        expect(container).toHaveClass("modal", "thing-dialog", "sheet");
        expect(container).not.toHaveClass("sheet-page");
    });

    it("fills the screen on a phone when asked to present as a page", () => {
        setViewportWidth(phoneWidth);

        const { container } = renderDialog({ phone: "page" });

        expect(container).toHaveClass("sheet", "sheet-page");
    });

    it("ignores the phone presentation on a desktop", () => {
        const { container } = renderDialog({ phone: "page" });

        expect(container).not.toHaveClass("sheet");
        expect(container).not.toHaveClass("sheet-page");
    });

    it("follows the viewport across the breakpoint while open", () => {
        const { container } = renderDialog();

        setViewportWidth(phoneWidth);
        expect(container).toHaveClass("sheet");

        setViewportWidth(1024);
        expect(container).not.toHaveClass("sheet");
    });

    it("closes from the header's button when the header is inside a form", async () => {
        setViewportWidth(phoneWidth);
        const { user, onHide } = renderDialog();

        await user.click(screen.getByRole("button", { name: "Close" }));

        expect(onHide).toHaveBeenCalledTimes(1);
    });

    it("closes on Escape as a sheet", async () => {
        setViewportWidth(phoneWidth);
        const { user, onHide } = renderDialog();

        await user.keyboard("{Escape}");

        expect(onHide).toHaveBeenCalledTimes(1);
    });

    it("moves focus into the sheet when it opens", () => {
        setViewportWidth(phoneWidth);
        renderDialog();

        expect(screen.getByRole("dialog")).toContainElement(document.activeElement as HTMLElement);
    });

    it("submits a form from the sheet's footer", async () => {
        setViewportWidth(phoneWidth);
        const { user, onSubmit } = renderDialog();

        await user.click(screen.getByRole("button", { name: "Save" }));

        expect(onSubmit).toHaveBeenCalledTimes(1);
    });
});
