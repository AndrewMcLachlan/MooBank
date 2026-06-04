import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";

export interface WidgetErrorProps {
    message?: string;
}

export const WidgetError: React.FC<WidgetErrorProps> = ({ message = "Unable to load this widget" }) => (
    <div className="widget-error">
        <FontAwesomeIcon icon="triangle-exclamation" className="widget-error-icon" />
        <div className="widget-error-message">{message}</div>
    </div>
);
