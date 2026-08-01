import { createFileRoute, redirect } from "@tanstack/react-router";

/**
 * Planning has no page of its own — it is a heading over the forecast and the retirement plan.
 *
 * The forecast is the default, so this sends you there rather than being the forecast, which keeps
 * the secondary navigation honest: the Forecast item points at a URL that is the forecast, and stays
 * highlighted whichever way you arrived.
 */
export const Route = createFileRoute("/planning/")({
    beforeLoad: () => {
        throw redirect({ to: "/planning/forecast" });
    },
});
