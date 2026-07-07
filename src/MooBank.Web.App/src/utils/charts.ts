/**
 * Calculates a y-axis tick step size of roughly a tenth of the data's magnitude.
 * Returns undefined for empty or all-zero datasets so Chart.js falls back to automatic ticks.
 */
export const getStepSize = (values: number[]): number | undefined => {
    if (!values.length) return undefined;
    const max = Math.max(...values.map(Math.abs));
    if (!Number.isFinite(max) || max === 0) return undefined;
    const magnitude = Math.pow(10, Math.floor(Math.log10(max)));
    const roundedMax = Math.ceil(max / magnitude) * magnitude;
    return roundedMax / 10;
};
