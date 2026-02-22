import { Form } from "@andrewmclachlan/moo-ds";
import { InstitutionSelector, useAccount } from "components";
import type { InstitutionAccount, LogicalAccount } from "api/types.gen";
import { Button, Modal } from "@andrewmclachlan/moo-ds";
import { useForm } from "react-hook-form";
import { useUpdateInstitutionAccount } from "../../../-hooks/useUpdateInstitutionAccount";
import { ImportSettings } from "./ImportSettings";
export const InstitutionAccountEdit: React.FC<InstitutionAccountEditProps> = ({ institutionAccount, show, onHide, onSave }) => {

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
            importerTypeId: data.importerTypeId,
            name: data.name,
        });
    }

    return (
        <Modal show={show} onHide={onHide} size="lg" title={institutionAccount ? "Edit Institution Account" : "Add Institution Account"} >
            <Form form={form} onSubmit={handleSubmit}>
                <Modal.Header closeButton>
                    <Modal.Title>{institutionAccount ? "Edit Institution Account" : "Add Institution Account"}</Modal.Title>
                </Modal.Header>
                <Modal.Body>
                    <Form.Group groupId="name">
                        <Form.Label>Name</Form.Label>
                        <Form.Input type="text" required maxLength={255} />
                    </Form.Group>
                    <Form.Group groupId="institutionId">
                        <Form.Label>Institution</Form.Label>
                        <InstitutionSelector accountType={account?.accountType} />
                    </Form.Group>
                    <Form.Group groupId="importerTypeId" hidden={account.controller !== "Import"}>
                        <Form.Label>Importer Type</Form.Label>
                        <ImportSettings />
                    </Form.Group>
                </Modal.Body>
                <Modal.Footer>
                    <Button variant="outline-primary" onClick={onHide}>Close</Button>
                    <Button type="submit" variant="primary" disabled={updateInstitutionAccount.isPending}>Save</Button>
                </Modal.Footer>
            </Form>
        </Modal >
    );
};

export interface InstitutionAccountEditProps {
    institutionAccount: InstitutionAccount;
    show: boolean;
    onHide: () => void;
    onSave: () => void;
}
