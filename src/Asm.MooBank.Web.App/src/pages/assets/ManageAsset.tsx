import React, { useEffect, useState } from "react";
import { Button, Form, InputGroup } from "react-bootstrap";
import { useNavigate } from "react-router-dom";
import { Asset } from "../../models";
import { AssetPage } from "./AssetPage";
import { useAsset } from "./AssetProvider";

import { useAccountGroups, useUpdateAsset } from "services";

export const ManageAsset: React.FC = () => {

    const navigate = useNavigate();

    const { data: accountGroups } = useAccountGroups();
    const updateAsset = useUpdateAsset();

    const currentAsset = useAsset();

    useEffect(() => {
        setAsset(currentAsset);
    }, [currentAsset]);

    const [asset, setAsset] = useState<Asset>();
    const handleSubmit = (e: React.FormEvent<HTMLFormElement>) => {
        e.stopPropagation();
        e.preventDefault();

        updateAsset(asset);

        navigate("/accounts");
    }

    if (!asset) return null;

    return (
        <AssetPage title="Manage" breadcrumbs={[{ text: "Manage", route: `/shares/${asset.id}/manage` }]}>
            <Form className="section" onSubmit={handleSubmit}>
                <Form.Group controlId="accountName" >
                    <Form.Label>Name</Form.Label>
                    <Form.Control type="text" required maxLength={50} value={asset.name} onChange={(e: any) => setAsset({ ...asset, name: e.currentTarget.value })} />
                    <Form.Control.Feedback type="invalid">Please enter a name</Form.Control.Feedback>
                </Form.Group>
                <Form.Group controlId="accountDescription" >
                    <Form.Label>Description</Form.Label>
                    <Form.Control type="text" as="textarea" required maxLength={255} value={asset.description} onChange={(e: any) => setAsset({ ...asset, description: e.currentTarget.value })} />
                    <Form.Control.Feedback type="invalid">Please enter a description</Form.Control.Feedback>
                </Form.Group>
                <Form.Group controlId="price" >
                    <Form.Label>Purchase Price</Form.Label>
                    <InputGroup>
                        <InputGroup.Text>$</InputGroup.Text>
                        <Form.Control type="number" required value={asset.purchasePrice?.toString()} onChange={(e: any) => setAsset({ ...asset, purchasePrice: e.currentTarget.value })} />
                    </InputGroup>
                </Form.Group>
                <Form.Group controlId="value" >
                    <Form.Label>Current Value</Form.Label>
                    <InputGroup>
                        <InputGroup.Text>$</InputGroup.Text>
                        <Form.Control type="number" required value={asset.currentBalance.toString()} onChange={(e: any) => setAsset({ ...asset, currentBalance: e.currentTarget.value})} />
                    </InputGroup>
                </Form.Group>
                <Form.Group controlId="group">
                    <Form.Label>Group</Form.Label>
                    <Form.Control as="select" value={asset.accountGroupId} onChange={(e: any) => setAsset({ ...asset, accountGroupId: e.currentTarget.value })}>
                        <option value="">Select a group...</option>
                        {accountGroups?.map(a =>
                            <option value={a.id} key={a.id}>{a.name}</option>
                        )}
                    </Form.Control>
                </Form.Group>
                <Form.Group controlId="ShareWithFamily">
                    <Form.Label>Visible to other family members</Form.Label>
                    <Form.Check checked={asset.shareWithFamily} onChange={(e: any) => setAsset({ ...asset, shareWithFamily: e.currentTarget.checked })} />
                </Form.Group>
                <Button type="submit" variant="primary">Update</Button>
            </Form>
        </AssetPage>
    );
}

ManageAsset.displayName = "ManageAsset";
