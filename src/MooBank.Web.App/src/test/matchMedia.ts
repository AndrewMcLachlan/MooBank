import { act } from "@testing-library/react";

export const desktopWidth = 1280;

let viewportWidth = desktopWidth;
const listeners = new Set<() => void>();

const minWidthOf = (query: string) => Number(/min-width:\s*(\d+)px/.exec(query)?.[1] ?? 0);

/**
 * Answers `min-width` media queries against a simulated viewport width, which jsdom has no notion
 * of. Supports what moo-ds's `useIsAtLeast` asks for and nothing more.
 */
export const installMatchMedia = () => {
    window.matchMedia = (query: string) => ({
        get matches() { return viewportWidth >= minWidthOf(query); },
        media: query,
        onchange: null,
        addEventListener: (_type: string, listener: () => void) => listeners.add(listener),
        removeEventListener: (_type: string, listener: () => void) => listeners.delete(listener),
        addListener: (listener: () => void) => listeners.add(listener),
        removeListener: (listener: () => void) => listeners.delete(listener),
        dispatchEvent: () => false,
    }) as unknown as MediaQueryList;
};

export const setViewportWidth = (width: number) => {
    viewportWidth = width;
    act(() => listeners.forEach(listener => listener()));
};

export const resetViewport = () => {
    viewportWidth = desktopWidth;
    listeners.clear();
};
