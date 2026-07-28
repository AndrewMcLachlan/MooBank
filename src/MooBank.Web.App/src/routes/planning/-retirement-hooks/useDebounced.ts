import { useEffect, useState } from "react";

/**
 * The value, held back until it has stopped changing for `delay` milliseconds.
 *
 * Dragging a slider produces a value on every frame, and each one would otherwise be a projection
 * request. The sliders themselves stay on the immediate value so they never feel laggy; only the
 * projection waits.
 */
export const useDebounced = <T,>(value: T, delay = 300): T => {
    const [settled, setSettled] = useState(value);

    useEffect(() => {
        const timer = setTimeout(() => setSettled(value), delay);
        return () => clearTimeout(timer);
    }, [value, delay]);

    return settled;
};
