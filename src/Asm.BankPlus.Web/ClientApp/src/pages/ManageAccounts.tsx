import React from "react";
import { connect } from "react-redux";

class ManageAccounts extends React.Component<any, any> {
    constructor(props) {
        super(props);
    }

    public render() {
        return (<section>Hello</section>);
    }
}

export default connect()(ManageAccounts);
