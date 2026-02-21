import { createFileRoute } from "@tanstack/react-router";
import React from "react";
import { Page } from "@andrewmclachlan/moo-app";

export const Route = createFileRoute("/$")({
    component: Error404,
});

function Error404() {

    return (
        <Page title="404">
            <h1>Page not found</h1>
        </Page>
    );
}
