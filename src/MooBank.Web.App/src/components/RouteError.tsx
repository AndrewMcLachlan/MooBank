import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";

export interface RouteErrorProps {
    error?: Error;
}

export const RouteError: React.FC<RouteErrorProps> = ({ error }) => (
    <div className="widget-error route-error">
        <FontAwesomeIcon icon="triangle-exclamation" className="widget-error-icon" />
        <div className="widget-error-message">Something went wrong. Try reloading the page.</div>
        {import.meta.env.DEV && error?.message && <pre className="route-error-detail">{error.message}</pre>}
    </div>
);
