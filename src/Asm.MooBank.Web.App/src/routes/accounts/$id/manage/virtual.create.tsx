import { createFileRoute } from "@tanstack/react-router";
import { Button } from "@andrewmclachlan/moo-ds";
import { useForm } from "react-hook-form";
import { useNavigate } from "@tanstack/react-router";

import { SectionForm, Form } from "@andrewmclachlan/moo-ds";

import type { CreateVirtualInstrument } from "api/types.gen";
import { useCreateVirtualInstrument } from "services";
import { AccountPage, CurrencyInput, useAccount } from "components";

export const Route = createFileRoute("/accounts/$id/manage/virtual/create")({
    component: CreateVirtualAccount,
});

function CreateVirtualAccount() {

    const navigate = useNavigate();

    const parentAccount = useAccount();

    const createVirtualAccount = useCreateVirtualInstrument();

    const handleSubmit = async (data: CreateVirtualInstrument) => {

        await createVirtualAccount.mutateAsync(parentAccount.id, data);

        navigate({ to: `/accounts/${parentAccount.id}/manage/` });
    }

    const form = useForm<CreateVirtualInstrument>({ defaultValues: { controller: "Virtual" } });

    const selectedController = form.watch("controller");

    return (
        <AccountPage title="Create Virtual Account" breadcrumbs={[{ text: "Manage", route: `/accounts/${parentAccount?.id}/manage` }, { text: "Create Virtual Account", route: `/accounts/${parentAccount?.id}/manage/virtual/create` }]}>
            <SectionForm form={form} onSubmit={handleSubmit}>
                <Form.Group groupId="name" >
                    <Form.Label>Name</Form.Label>
                    <Form.Input required maxLength={50} />
                </Form.Group>
                <Form.Group groupId="description" >
                    <Form.Label >Description</Form.Label>
                    <Form.TextArea maxLength={255} />
                </Form.Group>
                <Form.Group groupId="openingBalance" >
                    <Form.Label>Opening Balance</Form.Label>
                    <CurrencyInput />
                </Form.Group>
                <Form.Group groupId="controller">
                    <Form.Label>Type</Form.Label>
                    <div>
                        <div className="btn-group" role="group" aria-label="Controller selection">
                            <input type="radio" id="virtual" value="Virtual" className="btn-check" {...form.register("controller")} defaultChecked />
                            <label htmlFor="virtual" className="btn btn-outline-primary">Virtual</label>
                            <input type="radio" id="manual" value="Manual" className="btn-check" {...form.register("controller")} />
                            <label htmlFor="manual" className="btn btn-outline-primary">Reserved Sum</label>
                        </div>
                        <div>
                            <span className="small" hidden={selectedController !== "Virtual"}>A virtual transaction account. Use recurring transactions for regular top-ups</span>
                            <span className="small" hidden={selectedController !== "Manual"}>Simply reserve a sum of money for a future purpose</span>
                        </div>
                    </div>
                </Form.Group>
                <Button type="submit" variant="primary">Save</Button>
            </SectionForm>
        </AccountPage>
    );
}
