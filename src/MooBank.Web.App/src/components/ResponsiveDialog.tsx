import React, { createContext, use } from "react";
import { Modal, useIsAtLeast } from "@andrewmclachlan/moo-ds";
import type { ModalHeaderProps, ModalProps } from "@andrewmclachlan/moo-ds";

/**
 * How the dialog presents below the content breakpoint: a `sheet` rises from the bottom edge and
 * fits its content; a `page` fills the screen, for content too long to read in a sheet.
 */
export type PhonePresentation = "sheet" | "page";

export interface ResponsiveDialogProps extends ModalProps {
    phone?: PhonePresentation;
}

const HideContext = createContext<(() => void) | undefined>(undefined);

const ResponsiveDialogComponent: React.FC<React.PropsWithChildren<ResponsiveDialogProps>> = ({ phone = "sheet", className, onHide, children, ...rest }) => {

    const isPhone = !useIsAtLeast("md");

    const classes = [
        className,
        isPhone && "sheet",
        isPhone && phone === "page" && "sheet-page",
    ].filter(Boolean).join(" ");

    return (
        <HideContext value={onHide}>
            <Modal {...rest} onHide={onHide} className={classes || undefined}>
                {children}
            </Modal>
        </HideContext>
    );
};

ResponsiveDialogComponent.displayName = "ResponsiveDialog";

const ResponsiveDialogHeader: React.FC<React.PropsWithChildren<ModalHeaderProps>> = ({ onHide, ...rest }) => {
    const dialogOnHide = use(HideContext);
    return <Modal.Header {...rest} onHide={onHide ?? dialogOnHide} />;
};

ResponsiveDialogHeader.displayName = "ResponsiveDialog.Header";

/**
 * A moo-ds `Modal` that becomes a bottom sheet, or a full-screen page, on a phone.
 *
 * The header's close button takes `onHide` from the dialog, so it works when the header is nested
 * inside a form.
 */
export const ResponsiveDialog = Object.assign(ResponsiveDialogComponent, {
    Header: ResponsiveDialogHeader,
    Body: Modal.Body,
    Footer: Modal.Footer,
    Title: Modal.Title,
});
