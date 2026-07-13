import { describe, it, expect, afterEach, vi } from "vitest";
import { renderHook, act } from "@testing-library/react";
import { useMediaQuery, useIsDesktop } from "hooks/useMediaQuery";

// Minimal controllable MediaQueryList mock - jsdom doesn't implement matchMedia.
const createMatchMediaMock = (initialMatches: boolean) => {
    const listeners: ((event: MediaQueryListEvent) => void)[] = [];

    const mql = {
        matches: initialMatches,
        media: "",
        addEventListener: vi.fn((_event: string, callback: (event: MediaQueryListEvent) => void) => {
            listeners.push(callback);
        }),
        removeEventListener: vi.fn((_event: string, callback: (event: MediaQueryListEvent) => void) => {
            const index = listeners.indexOf(callback);
            if (index >= 0) listeners.splice(index, 1);
        }),
    };

    const fire = (matches: boolean) => {
        mql.matches = matches;
        listeners.forEach(listener => listener({ matches } as MediaQueryListEvent));
    };

    return { mql: mql as unknown as MediaQueryList, fire };
};

afterEach(() => {
    vi.restoreAllMocks();
});

describe("useMediaQuery", () => {
    it("returns the initial matches value from the mocked media query", () => {
        const { mql } = createMatchMediaMock(true);
        window.matchMedia = vi.fn().mockReturnValue(mql);

        const { result } = renderHook(() => useMediaQuery("(min-width: 768px)"));

        expect(result.current).toBe(true);
    });

    it("returns false when the query does not initially match", () => {
        const { mql } = createMatchMediaMock(false);
        window.matchMedia = vi.fn().mockReturnValue(mql);

        const { result } = renderHook(() => useMediaQuery("(min-width: 768px)"));

        expect(result.current).toBe(false);
    });

    it("updates when a change event fires", () => {
        const { mql, fire } = createMatchMediaMock(false);
        window.matchMedia = vi.fn().mockReturnValue(mql);

        const { result } = renderHook(() => useMediaQuery("(min-width: 768px)"));
        expect(result.current).toBe(false);

        act(() => fire(true));

        expect(result.current).toBe(true);
    });

    it("removes the change listener on unmount", () => {
        const { mql } = createMatchMediaMock(false);
        window.matchMedia = vi.fn().mockReturnValue(mql);

        const { unmount } = renderHook(() => useMediaQuery("(min-width: 768px)"));
        expect(mql.removeEventListener).not.toHaveBeenCalled();

        unmount();

        expect(mql.removeEventListener).toHaveBeenCalledWith("change", expect.any(Function));
    });
});

describe("useIsDesktop", () => {
    it("queries the md breakpoint and forwards the mocked matches value", () => {
        const { mql } = createMatchMediaMock(true);
        const matchMediaSpy = vi.fn().mockReturnValue(mql);
        window.matchMedia = matchMediaSpy;

        const { result } = renderHook(() => useIsDesktop());

        expect(matchMediaSpy).toHaveBeenCalledWith("(min-width: 768px)");
        expect(result.current).toBe(true);
    });

    it("returns false when the breakpoint does not match", () => {
        const { mql } = createMatchMediaMock(false);
        window.matchMedia = vi.fn().mockReturnValue(mql);

        const { result } = renderHook(() => useIsDesktop());

        expect(result.current).toBe(false);
    });
});
