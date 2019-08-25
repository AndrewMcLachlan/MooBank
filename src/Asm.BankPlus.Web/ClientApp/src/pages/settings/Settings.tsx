import React from "react";
import { Link } from "react-router-dom";

export const Settings: React.FC = () => {
    return (<section>
        <Link to="/settings/transaction-categories">Edit Transaction Categories</Link>
    </section>);
}