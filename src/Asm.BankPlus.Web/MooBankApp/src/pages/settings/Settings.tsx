import React from "react";
import { Link } from "react-router-dom";

export const Settings: React.FC = () => {
    return (<section>
        <Link to="/settings/tags">Edit Transaction Tags</Link>
    </section>);
}