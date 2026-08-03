/** Budget years live in the URL, so they have to survive whatever a user or a stale link supplies. */

const earliestBudgetYear = 1900;
const latestBudgetYear = 2200;

export const currentBudgetYear = () => new Date().getFullYear();

export const isBudgetYear = (value: string | number | undefined | null): boolean => {
    if (value === null || value === undefined || value === "") return false;

    const year = Number(value);

    return Number.isInteger(year) && year >= earliestBudgetYear && year <= latestBudgetYear;
};
