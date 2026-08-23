import { useCallback, useRef, useState } from "react";

/**
 * Tracks an element's rendered width.
 *
 * A media query answers how wide the viewport is, which is not the same question: the same chart
 * renders full width on a report page and inside one cell of the dashboard grid, so a widget can be
 * narrow on a wide screen and a viewport query would call it roomy.
 *
 * The returned ref is a callback ref, not an object ref, because consumers commonly hold the
 * element back on the first render — returning a loading skeleton until data arrives, say. React
 * calls this when the element attaches, whenever that happens, and again with null when it goes.
 * An effect keyed on an empty dependency array cannot do that: it runs once, finds nothing, and
 * never looks again.
 *
 * Width starts at null rather than 0, so a consumer can tell "not measured yet" from "measured and
 * genuinely tiny" and hold its default until the first observation.
 */
export const useElementWidth = <TElement extends HTMLElement = HTMLDivElement>() => {
    const [width, setWidth] = useState<number | null>(null);
    const observerRef = useRef<ResizeObserver | null>(null);
    const frameRef = useRef(0);

    const ref = useCallback((element: TElement | null) => {
        observerRef.current?.disconnect();
        cancelAnimationFrame(frameRef.current);

        if (!element) {
            observerRef.current = null;
            return;
        }

        // Measured here rather than left to the observer's first callback, which arrives a frame
        // later. A ref callback runs during commit, so the width is known before the browser
        // paints and a consumer can render itself correctly the first time instead of correcting
        // afterwards.
        const initial = element.getBoundingClientRect().width;
        if (initial > 0) setWidth(initial);

        // Coalesced into a frame: a resize drag fires continuously, and each distinct width would
        // otherwise re-render and re-lay out whatever is being measured.
        observerRef.current = new ResizeObserver(entries => {
            cancelAnimationFrame(frameRef.current);
            frameRef.current = requestAnimationFrame(() => {
                const observed = entries[0]?.contentRect?.width ?? 0;
                if (observed > 0) setWidth(observed);
            });
        });

        observerRef.current.observe(element);
    }, []);

    return [ref, width] as const;
};
