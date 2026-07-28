import { Link } from "@tanstack/react-router";

/**
 * Switches between the two things you can plan. Rendered as links rather than a stateful tab
 * control so each view keeps its own URL and stays shareable and bookmarkable.
 */
export const PlanningTabs: React.FC<PlanningTabsProps> = ({ active }) => (
    <nav className="planning-tabs" aria-label="Planning">
        <Link to="/planning" className="planning-tab" aria-current={active === "forecast" ? "page" : undefined}>
            Forecast
        </Link>
        <Link to="/planning/retirement" className="planning-tab" aria-current={active === "retirement" ? "page" : undefined}>
            Retirement
        </Link>
    </nav>
);

export interface PlanningTabsProps {
    active: "forecast" | "retirement";
}
