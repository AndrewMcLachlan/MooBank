import React from "react";
import { connect } from "react-redux";

class Settings extends React.Component<any, any> {
    constructor(props) {
        super(props);
    }

    public render() {
        return (<section>Settings</section>);
    }
}

export default connect()(Settings);
