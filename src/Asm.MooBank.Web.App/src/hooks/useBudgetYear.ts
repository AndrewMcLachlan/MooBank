import { useLocalStorage } from "@andrewmclachlan/moo-ds";

export const useBudgetYear = () => useLocalStorage("budget-year", new Date().getFullYear());