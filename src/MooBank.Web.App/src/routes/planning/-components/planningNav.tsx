import type { NavItem } from "@andrewmclachlan/moo-ds";
import { PiggyBank, Trendline } from "@andrewmclachlan/moo-icons";

/**
 * The two things you can plan, shown as secondary navigation under Planning.
 *
 * Defined once at module level rather than built per render: `Page` pushes `navItems`
 * into the layout with a reference comparison, so a fresh array each render would set
 * the context every time.
 */
export const planningNavItems: NavItem[] = [
    { route: "/planning", text: "Forecast", image: <Trendline /> },
    { route: "/planning/retirement", text: "Retirement", image: <PiggyBank /> },
];
