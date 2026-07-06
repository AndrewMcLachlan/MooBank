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
