import React, { useEffect } from "react";
import Table from "react-bootstrap-table-next";
import cellEditFactory from "react-bootstrap-table2-editor";
import { useDispatch, useSelector } from "react-redux";
import { bindActionCreators } from "redux";

import { actionCreators } from "store/TransactionTags";
import { State } from "store/state";

export const TransactionTags: React.FC = (props) => {

    var dispatch = useDispatch();
    bindActionCreators(actionCreators, dispatch);

    const categories = useSelector((state: State) => state.transactionTags.tags);

    useEffect(() => {
        dispatch(actionCreators.requestCategories());
    }, [props,dispatch]);

const cellEditF = cellEditFactory({ mode: "click"});

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
    );
}