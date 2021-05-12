import React from "react";

import { usePageTitle } from "../../hooks";

export const Error404: React.FC = () => {

    usePageTitle("404");

    return (
        <section>
            <h1>Page not found</h1>
        </section>
    );
}
