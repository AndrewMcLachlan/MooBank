import { ClickableIcon, EditColumn, Page } from "@andrewmclachlan/mooapp";
import { CurrencySelector } from "components";
import { AccountHolder, Card } from "models/AccountHolder";
import { useEffect, useState } from "react";
import { Button, Form, Table } from "react-bootstrap";
import Select from "react-select";
import { useAccountHolder, useAccountsList, useUpdateAccountHolder } from "services";

export const Profile: React.FC = () => {

    const { data: me } = useAccountHolder();
    const { data: accounts } = useAccountsList();
    const updateMe = useUpdateAccountHolder();

    const [updatedMe, setUpdatedMe] = useState<AccountHolder>(me);
    const [newCard, setNewCard] = useState<Card>({ name: "", last4Digits: null });

    useEffect(() => {
        setUpdatedMe(me);
    }, [me]);

    const createCard = () => {
        setUpdatedMe({ ...updatedMe, cards: [...updatedMe.cards, newCard].sort((a, b) => a.name?.localeCompare(b.name))});
        setNewCard({ name: "", last4Digits: null });
    }

    const deleteCard = (card:Card) => {
        setUpdatedMe({...updatedMe, cards: updatedMe.cards.filter(uc => uc.last4Digits !== card.last4Digits)})
    }

    const editCard = (index: number, card: Card) => {
        const cards = [...updatedMe.cards];
        cards[index] = card;
        setUpdatedMe({ ...updatedMe, cards: cards.sort((a, b) => a.name?.localeCompare(b.name))});
    }

    const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
        e.stopPropagation();
        e.preventDefault();
        updateMe(updatedMe);
    };

    if (!me || !updatedMe) return null;

    return (
        <Page title="Profile">
            <Form className="section" onSubmit={handleSubmit}>
                <Form.Group controlId="name">
                    <Form.Label>Name</Form.Label>
                    <Form.Control type="text" value={`${me.firstName} ${me.lastName}`} readOnly />
                </Form.Group>
                <Form.Group controlId="email">
                    <Form.Label>Email</Form.Label>
                    <Form.Control type="text" value={me.emailAddress} readOnly />
                </Form.Group>
                <Form.Group controlId="currency">
                    <Form.Label>Preferred Currency</Form.Label>
                    <CurrencySelector value={updatedMe.currency} onChange={e => setUpdatedMe({ ...updatedMe, currency: e.currentTarget.value })} />
                </Form.Group>
                <Form.Group controlId="primaryAccount">
                    <Form.Label>Primary Account (for the dashboard)</Form.Label>
                    <Select value={accounts?.find(a => a.id === updatedMe.primaryAccountId)} options={accounts} getOptionValue={o => o.id} getOptionLabel={o => o.name} onChange={v => setUpdatedMe({ ...updatedMe, primaryAccountId: v.id })} className="react-select" classNamePrefix="react-select" />
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
                            <td><input type="text" placeholder="Name" value={newCard.name} onChange={e => setNewCard({ ...newCard, name: e.currentTarget.value}) } className="form-control" /></td>
                            <td><input type="number" min={0} max={9999} maxLength={4} minLength={4} placeholder="Last 4 Digits" value={newCard.last4Digits ?? ""} onChange={e => setNewCard({ ...newCard, last4Digits: e.currentTarget.valueAsNumber})} className="form-control" /> </td>
                            <td className="row-action" onClick={createCard}><ClickableIcon icon="check-circle" title="Save" size="lg"/></td>
                        </tr>
                        {updatedMe.cards.map((c, index) => (
                            <tr key={index}>
                                <EditColumn value={c.name} onChange={t => editCard(index, {...c, name: t.value})}>{c.name}</EditColumn>
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
