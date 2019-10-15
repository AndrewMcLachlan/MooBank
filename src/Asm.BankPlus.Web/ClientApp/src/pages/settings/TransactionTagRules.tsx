import React, { useEffect } from "react";
import { Table } from "react-bootstrap";
import { useDispatch, useSelector } from "react-redux";
import { bindActionCreators } from "redux";

import { State } from "../../store/state";
import { actionCreators } from "../../store/TransactionTagRules";

import { TransactionTagRuleRow } from "../../components";

export const TransactionTagRules: React.FC = () => {

    var dispatch = useDispatch();
    bindActionCreators(actionCreators, dispatch);

    const rules = useSelector((state: State) => state.transactionTagRules.rules);

    useEffect(() => {
        dispatch(actionCreators.requestRules());
    }, [dispatch]);

    return (
        <Table striped bordered={false} borderless>
            <thead>
                <tr>
                    <th>When a transaction contains</th>
                    <th>Apply tag(s)</th>
                </tr>
            </thead>
            <tbody>
                <tr>
                    <td><input type="text" placeholder="Transaction description contains..." /></td>
                    <td></td>
                </tr>
                {rules && rules.map((r) => <TransactionTagRuleRow key={r.id} rule={r} />)}
            </tbody>
        </Table>

    );
}

TransactionTagRules.displayName = "TransactionTagRules";