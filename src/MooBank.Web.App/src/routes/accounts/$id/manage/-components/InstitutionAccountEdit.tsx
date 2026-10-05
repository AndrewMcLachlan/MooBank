import { Form } from "@andrewmclachlan/moo-ds";
import { InstitutionSelector, useAccount } from "components";
import { ResponsiveDialog } from "components/ResponsiveDialog";
import type { InstitutionAccount, LogicalAccount } from "api/types.gen";
import { Button } from "@andrewmclachlan/moo-ds";
import { useForm } from "react-hook-form";
import { useUpdateInstitutionAccount } from "../../../-hooks/useUpdateInstitutionAccount";

export const InstitutionAccountEdit: React.FC<InstitutionAccountEditProps> = ({ institutionAccount, show, onHide }) => {

    const account = useAccount() as LogicalAccount;
    const updateInstitutionAccount = useUpdateInstitutionAccount();

    const form = useForm<InstitutionAccount>({ defaultValues: institutionAccount });

    const handleSubmit = (data: InstitutionAccount) => {

        if (data.institutionId === undefined) {
            window.alert("Please select an institution");
            return;
        }

        updateInstitutionAccount.mutateAsync(account.id, institutionAccount.id, {
            institutionId: data.institutionId,
            name: data.name,
        });
    }

    return (
        <ResponsiveDialog show={show} onHide={onHide} size="lg" title={institutionAccount ? "Edit Institution Account" : "Add Institution Account"} >
            <Form form={form} onSubmit={handleSubmit}>
                <ResponsiveDialog.Header closeButton>
                    <ResponsiveDialog.Title>{institutionAccount ? "Edit Institution Account" : "Add Institution Account"}</ResponsiveDialog.Title>
                </ResponsiveDialog.Header>
                <ResponsiveDialog.Body>
                    <Form.Group groupId="name">
                        <Form.Label>Name</Form.Label>
                        <Form.Input type="text" required maxLength={255} />
                    </Form.Group>
                    <Form.Group groupId="institutionId">
                        <Form.Label>Institution</Form.Label>
                        <InstitutionSelector accountType={account?.accountType} />
                    </Form.Group>
                </ResponsiveDialog.Body>
                <ResponsiveDialog.Footer>
                    <Button variant="outline-primary" onClick={onHide}>Close</Button>
                    <Button type="submit" variant="primary" disabled={updateInstitutionAccount.isPending}>Save</Button>
                </ResponsiveDialog.Footer>
            </Form>
        </ResponsiveDialog >
    );
};

export interface InstitutionAccountEditProps {
    institutionAccount: InstitutionAccount;
    show: boolean;
    onHide: () => void;
    onSave: () => void;
}
