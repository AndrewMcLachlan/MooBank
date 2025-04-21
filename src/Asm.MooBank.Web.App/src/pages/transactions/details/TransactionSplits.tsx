import { Transaction, TransactionSplit } from "models";
import React, { useEffect, useState } from "react";
import { Col, Form, Row } from "react-bootstrap";
import { NewTransactionSplit } from "./NewTransactionSplit";
import { TransactionSplit as TransactionSplitPanel } from "./TransactionSplit";

export const TransactionSplits: React.FC<TransactionSplitsProps> = ({transaction, onChange}) => {

    const [splits, setSplits] = useState<TransactionSplit[]>(transaction.splits ?? []);

    useEffect(() => {
        onChange(splits);
    },[splits]);

    useEffect(() => {
        setSplits(transaction.splits ?? []);
    }, [transaction]);

    const addSplit = (split: TransactionSplit) => {
        setSplits([...splits, split]);
    }

    const splitChanged = (split: TransactionSplit) => {
        const newSplits = [...splits];
        newSplits.splice(newSplits.findIndex(o => o.id === split.id), 1, split);
        setSplits(newSplits);
    };

    const removeSplit = (id: string) => {
        const newSplits = splits.filter(s => s.id !== id);
        setSplits(newSplits);
    }

    return (
        <>
            <div>
                {splits?.map((split) =>
                    <React.Fragment key={split.id}>
                        <TransactionSplitPanel key={split.id} transaction={transaction} split={split} splitChanged={splitChanged} removeSplit={removeSplit} />
                    </React.Fragment>
                )}
            </div>
            <div>
                <Row>
                    <Col sm={9}>
                        <Form.Label>Split Transaction</Form.Label>
                    </Col>
                </Row>
                <NewTransactionSplit transaction={transaction} onSave={addSplit} />
            </div>
        </>
    )
}

export interface TransactionSplitsProps {
    transaction: Transaction;
    onChange: (splits: TransactionSplit[]) => void;
}
