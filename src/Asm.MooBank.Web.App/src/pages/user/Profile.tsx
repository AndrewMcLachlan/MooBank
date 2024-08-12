import { EditColumn, Form, Page, SectionForm } from "@andrewmclachlan/mooapp";
import { CurrencySelector } from "components";
import { DeleteIcon } from "components/DeleteIcon";
import { SaveIcon } from "components/SaveIcon";
import { Card, User } from "models/User";
import React, { useEffect, useState } from "react";
import { Button, Table } from "react-bootstrap";
import { Controller, useForm } from "react-hook-form";
import { useAccountsList, useUpdateUser, useUser } from "services";

export const Profile: React.FC = () => {

    const {data: me} = useUser();
    const { data: accounts } = useAccountsList();
    const updateMe = useUpdateUser();

    const [newCard, setNewCard] = useState<Card>({ name: "", last4Digits: null });

    const createCard = () => {
        const cards = getValues("cards");
        setValue("cards", [...cards, newCard].sort((a, b) => a.name?.localeCompare(b.name)));
        setNewCard({ name: "", last4Digits: null });
    }

    const deleteCard = (card: Card) => {
        const cards = getValues("cards");
        setValue("cards", cards.filter(uc => uc.last4Digits !== card.last4Digits));
    }

    const editCard = (index: number, card: Card) => {
        const cards = getValues("cards");
        cards[index] = card;
        setValue("cards", cards.sort((a, b) => a.name?.localeCompare(b.name)));
    }

    const handleSubmit = async (data: User) => {
        updateMe(data);
    };

    useEffect(() => {
        reset(me);
    }, [me, accounts]);

    const { register, setValue, getValues, reset, ...form } = useForm<User>({ defaultValues: me  });

    return (
        <Page title="Profile" breadcrumbs={[{text: "Profile", route: "/profile"}]}>
            <SectionForm onSubmit={form.handleSubmit(handleSubmit)}>
                <Form.Group groupId="name">
                    <Form.Label>Name</Form.Label>
                    <Form.Input type="text" value={`${me?.firstName ?? ""} ${me?.lastName ?? ""}`} readOnly />
                </Form.Group>
                <Form.Group groupId="email">
                    <Form.Label>Email</Form.Label>
                    <Form.Input type="text" defaultValue={me?.emailAddress} readOnly />
                </Form.Group>
                <Form.Group groupId="currency">
                    <Form.Label>Preferred Currency</Form.Label>
                    {!me && <Form.Select />}
                    {me &&
                    <Controller
                    control={form.control}
                    name="currency"
                        render={({ field: {onChange, value, ref} }) => (
                            <CurrencySelector value={value} ref={ref} onChange={onChange}   />
                    )}
                    />}
                </Form.Group>
                <Form.Group groupId="primaryAccount">
                    <Form.Label>Primary Account (for the dashboard)</Form.Label>
                    {!me && <Form.Select />}
                    {me && <Form.Select {...register("primaryAccountId")} className="form-select">
                        <option value="">Select an account</option>
                        {accounts?.map(a => <option key={a.id} value={a.id}>{a.name}</option>)}
                    </Form.Select>}
                    {/*<Select value={accounts?.find(a => a.id === updatedMe.primaryAccountId)} options={accounts} getOptionValue={o => o.id} getOptionLabel={o => o.name} onChange={v => setUpdatedMe({ ...updatedMe, primaryAccountId: v.id })} className="react-select" classNamePrefix="react-select" />*/}
                </Form.Group>
                <Table>
                    <thead>
                        <tr>
                            <th>Name</th>
                            <th>Last 4 Digits</th>
                            <th></th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr>
                            <td><input type="text" placeholder="Name" value={newCard.name} onChange={e => setNewCard({ ...newCard, name: e.currentTarget.value })} className="form-control" /></td>
                            <td><input type="number" min={0} max={9999} maxLength={4} minLength={4} placeholder="Last 4 Digits" value={newCard.last4Digits ?? ""} onChange={e => setNewCard({ ...newCard, last4Digits: e.currentTarget.valueAsNumber })} className="form-control" /> </td>
                            <td className="row-action" onClick={createCard}><SaveIcon /></td>
                        </tr>
                        {getValues("cards")?.map((c, index) => (
                            <tr key={index}>
                                <EditColumn value={c.name} onChange={t => editCard(index, { ...c, name: t.value })}>{c.name}</EditColumn>
                                <td>{c.last4Digits}</td>
                                <td className="row-action"><DeleteIcon onClick={_e => deleteCard(c)} /></td>
                            </tr>
                        )
                        )}
                    </tbody>
                </Table>
                <Button type="submit" variant="primary">Save</Button>
            </SectionForm>
        </Page>
    );
};
