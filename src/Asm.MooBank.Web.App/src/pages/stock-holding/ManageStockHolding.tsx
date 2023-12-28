import React, { useEffect, useState } from "react";
import { Form, Button, InputGroup, Col } from "react-bootstrap";
import { StockHolding, emptyStockHolding } from "../../models";
import { useStockHolding } from "./StockHoldingProvider";
import { StockHoldingPage } from "./StockHoldingPage";
import { useNavigate } from "react-router-dom";

import { useAccountGroups, useUpdateStockHolding } from "services";

export const ManageStockHolding: React.FC = () => {

    const navigate = useNavigate();

    const { data: accountGroups } = useAccountGroups();
    const updateStockHolding = useUpdateStockHolding();

    const currentStockHolding = useStockHolding();
    
    useEffect(() => {
        setStockHolding(currentStockHolding);
    }, [currentStockHolding]);

    const [stockHolding, setStockHolding] = useState<StockHolding>();
    const handleSubmit = (e: React.FormEvent<HTMLFormElement>) => {
        e.stopPropagation();
        e.preventDefault();

        updateStockHolding(stockHolding);

        navigate("/accounts");
    }

    if (!stockHolding) return null;

    return (
        <StockHoldingPage title="Manage" breadcrumbs={[{ text: "Manage", route: `/stock/${stockHolding.id}/manage` }]}>
            <Form className="section" onSubmit={handleSubmit}>
                <Form.Group controlId="accountName" >
                    <Form.Label>Name</Form.Label>
                    <Form.Control type="text" required maxLength={50} value={stockHolding.name} onChange={(e: any) => setStockHolding({ ...stockHolding, name: e.currentTarget.value })} />
                    <Form.Control.Feedback type="invalid">Please enter a name</Form.Control.Feedback>
                </Form.Group>
                <Form.Group controlId="accountDescription" >
                    <Form.Label>Description</Form.Label>
                    <Form.Control type="text" as="textarea" required maxLength={255} value={stockHolding.description} onChange={(e: any) => setStockHolding({ ...stockHolding, description: e.currentTarget.value })} />
                    <Form.Control.Feedback type="invalid">Please enter a description</Form.Control.Feedback>
                </Form.Group>
                <Form.Group controlId="symbol" >
                    <Form.Label>Symbol</Form.Label>
                    <Form.Control type="text" required maxLength={3} value={stockHolding.symbol} readOnly />
                </Form.Group>
                <Form.Group controlId="price" >
                    <Form.Label>Current Price</Form.Label>
                    <InputGroup>
                        <InputGroup.Text>$</InputGroup.Text>
                        <Form.Control type="number" required value={stockHolding.currentPrice?.toString()} onChange={(e: any) => setStockHolding({ ...stockHolding, currentPrice: e.currentTarget.value })} />
                    </InputGroup>
                </Form.Group>
                <Form.Group controlId="group">
                    <Form.Label>Group</Form.Label>
                    <Form.Control as="select" value={stockHolding.accountGroupId} onChange={(e: any) => setStockHolding({ ...stockHolding, accountGroupId: e.currentTarget.value })}>
                        <option value="">Select a group...</option>
                        {accountGroups?.map(a =>
                            <option value={a.id} key={a.id}>{a.name}</option>
                        )}
                    </Form.Control>
                </Form.Group>
                <Form.Group controlId="ShareWithFamily">
                    <Form.Label>Visible to other family members</Form.Label>
                    <Form.Check checked={stockHolding.shareWithFamily} onChange={(e: any) => setStockHolding({ ...stockHolding, shareWithFamily: e.currentTarget.checked })} />
                </Form.Group>
                <Button type="submit" variant="primary">Update</Button>
            </Form>
        </StockHoldingPage>
    );
}

ManageStockHolding.displayName = "ManageStockHolding";