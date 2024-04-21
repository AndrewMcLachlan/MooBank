import { ClickableIcon, EditColumn, Form, SectionForm, Page } from "@andrewmclachlan/mooapp";
import classNames from "classnames";
import { CurrencySelector } from "components";
import { AccountHolder, Card } from "models/AccountHolder";
import React from "react";
import { useEffect, useState } from "react";
import { Button, Table } from "react-bootstrap";
import { useForm } from "react-hook-form";
import Select from "react-select";
import { useAccountHolder, useAccountsList, useUpdateAccountHolder } from "services";

export const Profile: React.FC = () => {

    const {data: me} = useAccountHolder();
    const { data: accounts } = useAccountsList();
    const updateMe = useUpdateAccountHolder();

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

    const handleSubmit = async (data: AccountHolder) => {
        updateMe(data);
    };

    useEffect(() => {
        reset(me);
    }, [me, accounts]);

    const { register, setValue, getValues, reset, ...form } = useForm<AccountHolder>({ defaultValues: me  });

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
                    {me && <CurrencySelector {...register("currency")} />}
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
                            <td className="row-action" onClick={createCard}><ClickableIcon icon="check-circle" title="Save" size="lg" /></td>
                        </tr>
                        {getValues("cards")?.map((c, index) => (
                            <tr key={index}>
                                <EditColumn value={c.name} onChange={t => editCard(index, { ...c, name: t.value })}>{c.name}</EditColumn>
                                <td>{c.last4Digits}</td>
                                <td className="row-action"><ClickableIcon icon="trash-alt" title="Delete" size="1x" onClick={e => deleteCard(c)} /></td>
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


/*
import { ClickableIcon, EditColumn, Form, Page } from "@andrewmclachlan/mooapp";
import classNames from "classnames";
import { CurrencySelector } from "components";
import { AccountHolder, Card } from "models/AccountHolder";
import React from "react";
import { useEffect, useState } from "react";
import { Button, Table } from "react-bootstrap";
import { useForm } from "react-hook-form";
import Select from "react-select";
import { useAccountsList, useUpdateAccountHolder } from "services";

export const ProfileForm: React.FC<{me: AccountHolder}> = ({me}) => {

    const { data: accounts } = useAccountsList();
    const updateMe = useUpdateAccountHolder();

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

    const handleSubmit = async (data: AccountHolder) => {
        updateMe(data);
    };

    const { register, setValue, getValues, ...form } = useForm<AccountHolder>({ defaultValues: me  });

    return (
        <Page title="Profile">
            <Form className="section" onSubmit={form.handleSubmit(handleSubmit)}>
                <Form.Group groupId="name">
                    <Form.Label>Name</Form.Label>
                    <Form.Input type="text" value={`${me?.firstName} ${me?.lastName}`} readOnly />
                </Form.Group>
                <Form.Group groupId="email">
                    <Form.Label>Email</Form.Label>
                    <Form.Input type="text" defaultValue={me?.emailAddress} readOnly />
                </Form.Group>
                <Form.Group groupId="currency">
                    <Form.Label>Preferred Currency</Form.Label>
                    <CurrencySelector {...register("currency")} />
                </Form.Group>
                <Form.Group groupId="primaryAccount">
                    <Form.Label>Primary Account (for the dashboard)</Form.Label>
                    <Form.Select {...register("primaryAccountId")} className="form-select">
                        <option value="">Select an account</option>
                        {accounts?.map(a => <option key={a.id} value={a.id}>{a.name}</option>)}
                    </Form.Select>
                    {/*<Select value={accounts?.find(a => a.id === updatedMe.primaryAccountId)} options={accounts} getOptionValue={o => o.id} getOptionLabel={o => o.name} onChange={v => setUpdatedMe({ ...updatedMe, primaryAccountId: v.id })} className="react-select" classNamePrefix="react-select" />* /}
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
                                <td className="row-action" onClick={createCard}><ClickableIcon icon="check-circle" title="Save" size="lg" /></td>
                            </tr>
                            {getValues("cards")?.map((c, index) => (
                                <tr key={index}>
                                    <EditColumn value={c.name} onChange={t => editCard(index, { ...c, name: t.value })}>{c.name}</EditColumn>
                                    <td>{c.last4Digits}</td>
                                    <td className="row-action"><ClickableIcon icon="trash-alt" title="Delete" size="1x" onClick={e => deleteCard(c)} /></td>
                                </tr>
                            )
                            )}
                        </tbody>
                    </Table>
                    <Button type="submit" variant="primary">Save</Button>
                </Form>
            </Page>
        );
    };
*/
