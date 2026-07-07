import { useEffect, useState } from "react";

/**
 * Reactively tracks a CSS media query.
 */
export const useMediaQuery = (query: string): boolean => {
    const [matches, setMatches] = useState(() => window.matchMedia(query).matches);

    useEffect(() => {
        const mediaQueryList = window.matchMedia(query);
        const onChange = (e: MediaQueryListEvent) => setMatches(e.matches);
        setMatches(mediaQueryList.matches);
        mediaQueryList.addEventListener("change", onChange);
        return () => mediaQueryList.removeEventListener("change", onChange);
    }, [query]);

    return matches;
};

/**
 * True at or above the moo-ds `md` breakpoint (the same breakpoint the
 * `d-md-*` utility classes respond to), i.e. a desktop-sized viewport.
 */
export const useIsDesktop = (): boolean => useMediaQuery("(min-width: 768px)");
