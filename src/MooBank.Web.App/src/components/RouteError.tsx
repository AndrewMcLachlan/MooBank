import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { Button } from "@andrewmclachlan/moo-ds";
import { Page } from "@andrewmclachlan/moo-app";

export interface RouteErrorProps {
    error?: Error;
}

// Wrap in Page so the error occupies the content grid-area (main.container-fluid) —
// the same wrapper every route and moo-app's own Error page use. A bare div carries
// no grid-area and gets dropped into an implicit cell (under the sidebar).
export const RouteError: React.FC<RouteErrorProps> = ({ error }) => (
    <Page title="Error">
        <div className="route-error" role="alert">
            <div className="route-error-content">
                <span className="route-error-icon" aria-hidden="true">
                    <FontAwesomeIcon icon="triangle-exclamation" />
                </span>
                <h1 className="route-error-title">Something went wrong</h1>
                <p className="route-error-message">The page didn&rsquo;t load properly. Reloading usually fixes it.</p>
                <Button variant="primary" onClick={() => window.location.reload()}>Reload page</Button>
                {import.meta.env.DEV && error?.message && (
                    <pre className="route-error-detail">{error.message}</pre>
                )}
            </div>
        </div>
    </Page>
);
