import React, { useEffect } from "react";

import { useDispatch, useSelector } from "react-redux";
import { bindActionCreators } from "redux";

import { actionCreators } from "store/TransactionTags";
import { State } from "store/state";
import { TransactionTagRow } from "components";
import { Table } from "react-bootstrap";

export const TransactionTags: React.FC = (props) => {

    var dispatch = useDispatch();
    bindActionCreators(actionCreators, dispatch);

    const tags = useSelector((state: State) => state.transactionTags.tags);

    useEffect(() => {
        dispatch(actionCreators.requestTags());
    }, [props,dispatch]);

    /*const cellEditF = cellEditFactory({ mode: "click"});

    const columns = [{
        dataField: "id",
        text: "",
        hidden: true,
    }, {
        dataField: "name",
        text: "Name",
        sort: true,
        headerStyle: () => { return { "text-align": "left", width: "35%" }; }
    }, {
        dataField: "isLivingExpense",
        text: "Is Living Expense",
        sort: true,
        editor: {
            type: "CHECKBOX",
        },
        headerStyle: () => { return { width: "10%" }; }
    }];

    return (
        <section>
            <Table keyField="id"
                data={categories}
                columns={columns}
                bordered={false}
                striped
                bootstrap4
                cellEdit={ cellEditFactory({ mode: 'click' }) } />
        </section>
    );*/

    return (
        <Table striped bordered={false} borderless>
        <thead>
            <tr>
                <th>Name</th>
                <th>Tags</th>
            </tr>
        </thead>
        <tbody>
            {tags && tags.map((t) => <TransactionTagRow key={t.id} tag={t} />) }
        </tbody>
    </Table>
    );
}