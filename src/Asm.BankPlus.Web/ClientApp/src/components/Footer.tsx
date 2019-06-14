import React from "react";
import { connect } from "react-redux";

const Footer = (props) => (
    <footer>
        Copyright &copy; Andrew McLachlan 2013. All rights reserved.<br />
        <a href="http://www.andrewmclachlan.com">www.andrewmclachlan.com</a>
    </footer>
);

export default connect()(Footer);
