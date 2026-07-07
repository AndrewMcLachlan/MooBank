import { createRootRoute } from "@tanstack/react-router";
import { useQueryClient } from "@tanstack/react-query";
import { useEffect } from "react";

import { getTagsOptions, importerTypesOptions } from "api/@tanstack/react-query.gen";
import Layout from "../Layout";

const Root = () => {

    const queryClient = useQueryClient();

    // The root route only renders once login has completed, so this warms
    // caches the user will need shortly (tag panels, import page) while they
    // are looking at the dashboard.
    useEffect(() => {
        queryClient.prefetchQuery({ ...getTagsOptions(), staleTime: 60 * 1000 });
        queryClient.prefetchQuery({ ...importerTypesOptions(), staleTime: 60 * 1000 });
    }, [queryClient]);

    return <Layout />;
};

export const Route = createRootRoute({
    component: Root,
});
